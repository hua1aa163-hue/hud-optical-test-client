namespace HudOpticalTestClient.Networking;

/// <summary>TCP 连接结束的原因。</summary>
public enum ConnectionClosedReason
{
    /// <summary>用户或上层业务主动调用了 DisconnectAsync。</summary>
    LocalDisconnect,

    /// <summary>ConnectAsync 被再次调用，旧连接被新连接替换。</summary>
    Reconnecting,

    /// <summary>服务器正常关闭了 TCP 字节流。</summary>
    RemoteClosed,

    /// <summary>收发过程中发生了套接字或数据流异常。</summary>
    TransportError,

    /// <summary>TcpMessageClient 正在释放。</summary>
    Disposed
}

/// <summary>为 <see cref="TcpMessageClient.ConnectionClosed"/> 提供连接关闭信息。</summary>
public sealed class ConnectionClosedEventArgs : EventArgs
{
    internal ConnectionClosedEventArgs(
        ConnectionClosedReason reason,
        long connectionGeneration,
        Exception? exception)
    {
        Reason = reason;
        ConnectionGeneration = connectionGeneration;
        Exception = exception;
    }

    /// <summary>连接结束原因。</summary>
    public ConnectionClosedReason Reason { get; }

    /// <summary>
    /// 连接代次。每次成功 ConnectAsync 都会递增，可用于区分重连前后的事件。
    /// </summary>
    public long ConnectionGeneration { get; }

    /// <summary>因传输异常关闭时的原始异常；正常关闭时为 null。</summary>
    public Exception? Exception { get; }

    /// <summary>适合直接显示在日志中的中文说明。</summary>
    public string Description => Reason switch
    {
        ConnectionClosedReason.LocalDisconnect => "已主动断开连接",
        ConnectionClosedReason.Reconnecting => "旧连接已由重连替换",
        ConnectionClosedReason.RemoteClosed => "服务器已关闭连接",
        ConnectionClosedReason.TransportError => Exception is null
            ? "TCP 连接因传输错误关闭"
            : $"TCP 连接因传输错误关闭：{Exception.Message}",
        ConnectionClosedReason.Disposed => "TCP 客户端已释放",
        _ => "TCP 连接已关闭"
    };
}
