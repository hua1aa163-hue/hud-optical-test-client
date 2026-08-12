using System.Net.Sockets;
using System.Text;

namespace HudOpticalTestClient.Networking;

/// <summary>
/// 主动连接 HUD 光学软件的单连接 TCP 客户端。
/// </summary>
/// <remarks>
/// TCP 只提供字节流，不保证一次读取正好是一条消息。本类以字符“%”作为返回消息终止符，
/// 因而能同时处理半包和粘包。界面只需订阅事件，不应自行读取 NetworkStream。
/// </remarks>
public sealed class TcpMessageClient : IAsyncDisposable
{
    private const int ReceiveBufferSize = 8 * 1024;
    private const int MaxPendingCharacters = 1024 * 1024;

    private readonly object _stateLock = new();
    private readonly SemaphoreSlim _lifecycleLock = new(1, 1);
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly Encoding _encoding;

    private ConnectionContext? _connection;
    private CancellationTokenSource? _pendingConnectCancellation;
    private long _lastGeneration;
    private bool _disposed;

    /// <summary>使用 UTF-8（无 BOM）收发文本。</summary>
    public TcpMessageClient()
        : this(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
    {
    }

    /// <summary>使用指定字符编码收发文本。</summary>
    public TcpMessageClient(Encoding encoding)
    {
        _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
    }

    /// <summary>成功建立一代新连接后触发。</summary>
    public event EventHandler? Connected;

    /// <summary>每拆出一条以“%”结尾的完整返回消息后触发。</summary>
    public event EventHandler<string>? MessageReceived;

    /// <summary>
    /// 每一代已建立的连接只触发一次；主动断开、远端断开、重连替换和异常均会触发。
    /// </summary>
    public event EventHandler<ConnectionClosedEventArgs>? ConnectionClosed;

    /// <summary>当前是否存在尚未关闭的已建立连接。</summary>
    public bool IsConnected
    {
        get
        {
            lock (_stateLock)
            {
                return _connection is { IsCloseSignaled: false };
            }
        }
    }

    /// <summary>当前连接的主机名；未连接时为 null。</summary>
    public string? RemoteHost
    {
        get
        {
            lock (_stateLock)
            {
                return _connection?.Host;
            }
        }
    }

    /// <summary>当前连接的端口；未连接时为 null。</summary>
    public int? RemotePort
    {
        get
        {
            lock (_stateLock)
            {
                return _connection?.Port;
            }
        }
    }

    /// <summary>
    /// 主动连接服务器。若当前已有连接，会先以“重连替换”原因完整关闭旧连接。
    /// </summary>
    public async Task ConnectAsync(
        string host,
        int port,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new ArgumentException("服务器地址不能为空。", nameof(host));
        }

        if (port is < 1 or > 65535)
        {
            throw new ArgumentOutOfRangeException(nameof(port), "端口必须在 1 到 65535 之间。");
        }

        await _lifecycleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        ConnectionContext? newConnection = null;
        try
        {
            ThrowIfDisposed();

            ConnectionContext? oldConnection;
            lock (_stateLock)
            {
                oldConnection = _connection;
                if (oldConnection is not null)
                {
                    _connection = null;
                }
            }

            if (oldConnection is not null)
            {
                await CloseContextAsync(
                    oldConnection,
                    ConnectionClosedReason.Reconnecting,
                    exception: null,
                    waitForReceiveLoop: true).ConfigureAwait(false);
            }

            using var pendingCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            lock (_stateLock)
            {
                ThrowIfDisposed();
                _pendingConnectCancellation = pendingCancellation;
            }

            var tcpClient = new TcpClient
            {
                NoDelay = true
            };

            try
            {
                await tcpClient.ConnectAsync(host.Trim(), port, pendingCancellation.Token)
                    .ConfigureAwait(false);
                pendingCancellation.Token.ThrowIfCancellationRequested();

                NetworkStream stream = tcpClient.GetStream();
                lock (_stateLock)
                {
                    ThrowIfDisposed();
                    long generation = ++_lastGeneration;
                    newConnection = new ConnectionContext(
                        generation,
                        host.Trim(),
                        port,
                        tcpClient,
                        stream);
                    _connection = newConnection;
                }

                // 接收任务先建立但在门闩处等待。这样 Connected 永远先于本代的关闭事件，
                // 同时 Connected 处理器内同步调用 DisconnectAsync 也不会形成死锁。
                newConnection.ReceiveTask = ReceiveAfterStartSignalAsync(newConnection);
            }
            catch
            {
                tcpClient.Dispose();
                throw;
            }
            finally
            {
                lock (_stateLock)
                {
                    if (ReferenceEquals(_pendingConnectCancellation, pendingCancellation))
                    {
                        _pendingConnectCancellation = null;
                    }
                }
            }
        }
        finally
        {
            _lifecycleLock.Release();
        }

        // 不在生命周期锁内调用用户事件，避免事件处理器反向调用连接 API 时死锁。
        if (newConnection is not null)
        {
            // 先标记“Connected 事件即将开始”，关闭路径据此保证事件顺序。
            newConnection.ConnectedEventStarted.TrySetResult();
            RaiseConnected();
            newConnection.ReceiveStartSignal.TrySetResult();
        }
    }

    /// <summary>主动断开当前连接；未连接时可安全重复调用。</summary>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        // 若 DNS/连接建立尚未完成，先让该 ConnectAsync 尽快退出，再等待生命周期锁。
        lock (_stateLock)
        {
            _pendingConnectCancellation?.Cancel();
        }

        await _lifecycleLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ConnectionContext? connection;
            lock (_stateLock)
            {
                connection = _connection;
                if (connection is not null)
                {
                    _connection = null;
                }
            }

            if (connection is not null)
            {
                await CloseContextAsync(
                    connection,
                    ConnectionClosedReason.LocalDisconnect,
                    exception: null,
                    waitForReceiveLoop: true).ConfigureAwait(false);
            }
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    /// <summary>向当前服务器发送一段文本。</summary>
    /// <remarks>
    /// 发送锁保证并发调用不会让两条命令的字节互相穿插。是否需要“%”由具体请求协议决定；
    /// HUD 的 t1～t21 请求本身不带“%”，因此网络层不会擅自追加字符。
    /// </remarks>
    public async Task SendAsync(string message, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message.Length == 0)
        {
            throw new ArgumentException("发送内容不能为空。", nameof(message));
        }

        ConnectionContext connection;
        lock (_stateLock)
        {
            ThrowIfDisposed();
            connection = _connection
                ?? throw new InvalidOperationException("尚未连接服务器，无法发送。 ");
        }

        bool lockTaken = false;
        try
        {
            await _sendLock.WaitAsync(cancellationToken).ConfigureAwait(false);
            lockTaken = true;

            lock (_stateLock)
            {
                ThrowIfDisposed();
                if (!ReferenceEquals(_connection, connection) || connection.IsCloseSignaled)
                {
                    throw new InvalidOperationException("连接已经变化或关闭，请重新发送。 ");
                }
            }

            byte[] bytes = _encoding.GetBytes(message);
            await connection.Stream.WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
            await connection.Stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // 用户取消单次发送不等于 TCP 已损坏，因此保留连接。
            throw;
        }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException)
        {
            await CloseContextAsync(
                connection,
                ConnectionClosedReason.TransportError,
                ex,
                waitForReceiveLoop: true).ConfigureAwait(false);
            throw;
        }
        finally
        {
            if (lockTaken)
            {
                _sendLock.Release();
            }
        }
    }

    /// <summary>释放客户端并关闭当前连接。</summary>
    public async ValueTask DisposeAsync()
    {
        lock (_stateLock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _pendingConnectCancellation?.Cancel();
        }

        await _lifecycleLock.WaitAsync().ConfigureAwait(false);
        try
        {
            ConnectionContext? connection;
            lock (_stateLock)
            {
                connection = _connection;
                _connection = null;
            }

            if (connection is not null)
            {
                await CloseContextAsync(
                    connection,
                    ConnectionClosedReason.Disposed,
                    exception: null,
                    waitForReceiveLoop: true).ConfigureAwait(false);
            }
        }
        finally
        {
            _lifecycleLock.Release();
        }
    }

    private async Task ReceiveAfterStartSignalAsync(ConnectionContext connection)
    {
        await connection.ReceiveStartSignal.Task.ConfigureAwait(false);
        await ReceiveLoopAsync(connection).ConfigureAwait(false);
    }

    private async Task ReceiveLoopAsync(ConnectionContext connection)
    {
        byte[] byteBuffer = new byte[ReceiveBufferSize];
        char[] charBuffer = new char[_encoding.GetMaxCharCount(ReceiveBufferSize)];
        Decoder decoder = _encoding.GetDecoder();
        var pending = new StringBuilder();

        try
        {
            while (true)
            {
                int byteCount = await connection.Stream
                    .ReadAsync(byteBuffer, connection.Cancellation.Token)
                    .ConfigureAwait(false);

                if (byteCount == 0)
                {
                    await CloseContextAsync(
                        connection,
                        ConnectionClosedReason.RemoteClosed,
                        exception: null,
                        waitForReceiveLoop: false).ConfigureAwait(false);
                    return;
                }

                int bytesUsed = 0;
                while (bytesUsed < byteCount)
                {
                    decoder.Convert(
                        byteBuffer,
                        bytesUsed,
                        byteCount - bytesUsed,
                        charBuffer,
                        0,
                        charBuffer.Length,
                        flush: false,
                        out int convertedBytes,
                        out int convertedChars,
                        out _);

                    bytesUsed += convertedBytes;
                    if (convertedChars > 0)
                    {
                        pending.Append(charBuffer, 0, convertedChars);
                        PublishCompleteMessages(pending);
                    }
                }

                if (pending.Length > MaxPendingCharacters)
                {
                    throw new InvalidDataException(
                        "连续收到超过 1 MiB 且没有“%”终止符的数据，已中止连接以防止内存无限增长。 ");
                }
            }
        }
        catch (OperationCanceledException) when (connection.Cancellation.IsCancellationRequested)
        {
            // 主动关闭流程负责发送唯一一次 ConnectionClosed 事件。
        }
        catch (ObjectDisposedException) when (connection.Cancellation.IsCancellationRequested)
        {
            // 主动关闭 NetworkStream 会唤醒 ReadAsync，这是正常退出路径。
        }
        catch (Exception ex) when (ex is IOException or SocketException or InvalidDataException)
        {
            await CloseContextAsync(
                connection,
                ConnectionClosedReason.TransportError,
                ex,
                waitForReceiveLoop: false).ConfigureAwait(false);
        }
    }

    private void PublishCompleteMessages(StringBuilder pending)
    {
        while (true)
        {
            int terminatorIndex = IndexOf(pending, '%');
            if (terminatorIndex < 0)
            {
                return;
            }

            int messageLength = terminatorIndex + 1;
            string message = pending.ToString(0, messageLength);
            pending.Remove(0, messageLength);
            RaiseMessageReceived(message);
        }
    }

    private static int IndexOf(StringBuilder builder, char value)
    {
        for (int index = 0; index < builder.Length; index++)
        {
            if (builder[index] == value)
            {
                return index;
            }
        }

        return -1;
    }

    private async Task CloseContextAsync(
        ConnectionContext connection,
        ConnectionClosedReason reason,
        Exception? exception,
        bool waitForReceiveLoop)
    {
        lock (_stateLock)
        {
            // 只允许这一代连接移除自己；旧接收循环绝不能破坏重连后的新连接。
            if (ReferenceEquals(_connection, connection))
            {
                _connection = null;
            }
        }

        try
        {
            connection.Cancellation.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // 另一条关闭路径已完成。
        }

        connection.ReceiveStartSignal.TrySetResult();

        try
        {
            connection.Stream.Dispose();
        }
        catch
        {
            // 关闭阶段不让 Dispose 的次要异常覆盖最初原因。
        }

        try
        {
            connection.Client.Dispose();
        }
        catch
        {
            // 同上。
        }

        Task? receiveTask = connection.ReceiveTask;
        if (waitForReceiveLoop && receiveTask is not null)
        {
            try
            {
                await receiveTask.ConfigureAwait(false);
            }
            catch
            {
                // 接收循环会把传输异常转换成 ConnectionClosed；关闭调用本身应保持幂等。
            }
        }

        // 极端并发下，新一代 ConnectAsync 可能在上一代刚释放生命周期锁时立刻替换它。
        // 等待 Connected 开始后再通知 Closed，避免用户看到“先关闭、后连接”的倒序事件。
        await connection.ConnectedEventStarted.Task.ConfigureAwait(false);

        if (connection.TrySignalClosed())
        {
            RaiseConnectionClosed(new ConnectionClosedEventArgs(
                reason,
                connection.Generation,
                exception));
            connection.Cancellation.Dispose();
        }
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TcpMessageClient));
        }
    }

    private void RaiseConnected()
    {
        InvokeHandlersSafely(Connected, handler => handler(this, EventArgs.Empty));
    }

    private void RaiseMessageReceived(string message)
    {
        InvokeHandlersSafely(MessageReceived, handler => handler(this, message));
    }

    private void RaiseConnectionClosed(ConnectionClosedEventArgs args)
    {
        InvokeHandlersSafely(ConnectionClosed, handler => handler(this, args));
    }

    private static void InvokeHandlersSafely<THandler>(
        THandler? multicastDelegate,
        Action<THandler> invoke)
        where THandler : Delegate
    {
        if (multicastDelegate is null)
        {
            return;
        }

        foreach (Delegate handler in multicastDelegate.GetInvocationList())
        {
            try
            {
                invoke((THandler)handler);
            }
            catch
            {
                // 网络状态不能被某个界面日志处理器的异常打断。
            }
        }
    }

    private sealed class ConnectionContext
    {
        private int _closeSignaled;

        public ConnectionContext(
            long generation,
            string host,
            int port,
            TcpClient client,
            NetworkStream stream)
        {
            Generation = generation;
            Host = host;
            Port = port;
            Client = client;
            Stream = stream;
        }

        public long Generation { get; }
        public string Host { get; }
        public int Port { get; }
        public TcpClient Client { get; }
        public NetworkStream Stream { get; }
        public CancellationTokenSource Cancellation { get; } = new();
        public TaskCompletionSource ReceiveStartSignal { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource ConnectedEventStarted { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task? ReceiveTask { get; set; }
        public bool IsCloseSignaled => Volatile.Read(ref _closeSignaled) != 0;

        public bool TrySignalClosed()
        {
            return Interlocked.Exchange(ref _closeSignaled, 1) == 0;
        }
    }
}
