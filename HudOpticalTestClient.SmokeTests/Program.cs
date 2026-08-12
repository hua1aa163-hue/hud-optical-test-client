using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using HudOpticalTestClient;
using HudOpticalTestClient.Networking;
using HudOpticalTestClient.Protocol;

namespace HudOpticalTestClient.SmokeTests;

internal static class Program
{
    private static readonly TimeSpan AsyncTimeout = TimeSpan.FromSeconds(5);

    [STAThread]
    private static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        (string Name, Func<Task> Body)[] tests =
        [
            ("26 项协议目录与可视字段", () => RunSync(TestCatalog)),
            ("命令构建、t11 成对参数与原文校验", () => RunSync(TestMessageBuildingAndValidation)),
            ("响应成功/失败/忽略与超时策略", () => RunSync(TestResponseMatchingAndTimeouts)),
            ("真实 TCP 主动连接、分包粘包、断开与重连", TestTcpMessageClientAsync),
            ("STA MainForm 布局、默认值与投影顺序", () => RunSync(TestMainFormOnStaThread))
        ];

        int passed = 0;
        foreach ((string name, Func<Task> body) in tests)
        {
            try
            {
                await body();
                passed++;
                Console.WriteLine($"[PASS] {name}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[FAIL] {name}");
                Console.Error.WriteLine(ex);
            }
        }

        Console.WriteLine($"SmokeTests: {passed}/{tests.Length} passed.");
        if (passed == tests.Length && args.Contains("--render-ui", StringComparer.OrdinalIgnoreCase))
        {
            string outputDirectory = args.Length > 1
                ? Path.GetFullPath(args[1])
                : Path.Combine(AppContext.BaseDirectory, "ui-snapshots");
            int width = args.Length > 2 && int.TryParse(args[2], out int parsedWidth)
                ? parsedWidth
                : 1440;
            int height = args.Length > 3 && int.TryParse(args[3], out int parsedHeight)
                ? parsedHeight
                : 900;
            RenderUiSnapshots(outputDirectory, new Size(width, height));
            Console.WriteLine($"UI snapshots: {outputDirectory}");
        }
        return passed == tests.Length ? 0 : 1;
    }

    private static Task RunSync(Action action)
    {
        action();
        return Task.CompletedTask;
    }

    private static void TestCatalog()
    {
        AssertEqual(26, HudCommandCatalog.All.Count, "协议目录总数");
        AssertEqual(21, HudCommandCatalog.Tests.Count, "光学测试数");
        AssertEqual(5, HudCommandCatalog.AuxiliaryCommands.Count, "辅助指令数");

        string[] expectedTests = Enumerable.Range(1, 21).Select(index => $"t{index}").ToArray();
        AssertSequenceEqual(expectedTests, HudCommandCatalog.Tests.Select(item => item.Code),
            "t1～t21 目录顺序");
        AssertSequenceEqual(["c-", "gin", "pic-", "bmp-", "n-"],
            HudCommandCatalog.AuxiliaryCommands.Select(item => item.Code), "辅助指令顺序");

        AssertEqual(26, HudCommandCatalog.All.Select(item => item.Code)
            .Distinct(StringComparer.OrdinalIgnoreCase).Count(), "命令代码必须唯一");
        Assert(HudCommandCatalog.TryGet(" T11 ", out TestDefinition t11) && t11.Code == "t11",
            "TryGet 应忽略代码首尾空白和大小写");
        Assert(ReferenceEquals(t11, HudCommandCatalog.Get("t11")), "Get/TryGet 应返回同一定义");
        Assert(!HudCommandCatalog.TryGet("t99", out _), "未知命令不应命中目录");
        AssertThrows<KeyNotFoundException>(() => HudCommandCatalog.Get("t99"),
            "Get 未知命令");

        AssertEqual(18, HudCommandCatalog.Get("t1").ResponseFields.Count, "t1 规范字段数");
        AssertEqual(17, HudCommandCatalog.Get("t2").ResponseFields.Count, "t2 规范字段数");
        AssertEqual(29, HudCommandCatalog.Get("t3").ResponseFields.Count, "t3 字段数");
        AssertEqual(29, HudCommandCatalog.Get("t4").ResponseFields.Count, "t4 字段数");
        AssertEqual(29, HudCommandCatalog.Get("t21").ResponseFields.Count, "t21 字段数");
        AssertEqual(21, HudCommandCatalog.Get("t20").ResponseFields.Count, "t20 字段数");
        AssertEqual("mrad", HudCommandCatalog.Get("t11").ResponseFields[0].Unit,
            "t11 输出单位");

        foreach (TestDefinition definition in HudCommandCatalog.All)
        {
            Assert(!string.IsNullOrWhiteSpace(definition.Name), $"{definition.Code} 缺少中文名称");
            Assert(!string.IsNullOrWhiteSpace(definition.Description),
                $"{definition.Code} 缺少协议说明");
            Assert(!string.IsNullOrWhiteSpace(definition.ResponseHint),
                $"{definition.Code} 缺少返回提示");

            var defaults = definition.Fields.ToDictionary(
                field => field.Key,
                field => (string?)field.DefaultValue,
                StringComparer.OrdinalIgnoreCase);
            Assert(HudMessageProtocol.TryBuildMessage(
                    definition.Code, defaults, out string built, out string error),
                $"{definition.Code} 默认字段无法构建：{error}");
            AssertEqual(definition.DefaultMessage, built, $"{definition.Code} 默认报文");
            Assert(HudMessageProtocol.ValidateRaw(built, out error),
                $"{definition.Code} 默认报文未通过原文校验：{error}");
        }
    }

    private static void TestMessageBuildingAndValidation()
    {
        AssertEqual("t1", HudMessageProtocol.BuildMessage("t1"), "t1 构建");
        AssertEqual("gin", HudMessageProtocol.BuildMessage("gin"), "gin 构建");
        AssertEqual("c-qwert", HudMessageProtocol.BuildMessage("c-", Fields(("recipe", "qwert"))),
            "动态配方命令");
        AssertEqual(@"pic-D:\1\2.jpg",
            HudMessageProtocol.BuildMessage("pic-", Fields(("filePath", @"D:\1\2.jpg"))),
            "动态 pic 命令");
        AssertEqual(@"bmp-D:\1|str", HudMessageProtocol.BuildMessage(
            "bmp-", Fields(("directory", @"D:\1"), ("baseName", "str"))),
            "动态 bmp 命令");
        AssertEqual("n-asdfg,%", HudMessageProtocol.BuildMessage(
            "n-", Fields(("workbookName", "asdfg"))), "动态 n 命令");

        AssertEqual("t11", HudMessageProtocol.BuildMessage("t11"), "t11 无参命令");
        AssertEqual("t11/10/20", HudMessageProtocol.BuildMessage(
            "t11", Fields(("xTranslation", "10"), ("yTranslation", "20"))),
            "t11 X/Y 成对命令");
        Assert(HudMessageProtocol.TryGetDefinition("t11/10/20", out TestDefinition t11) &&
               t11.Code == "t11", "带参 t11 应映射回 t11 定义");

        Assert(!HudMessageProtocol.TryBuildMessage(
                "t11", Fields(("xTranslation", "10")), out _, out string pairError) &&
               pairError.Contains("同时", StringComparison.Ordinal),
            "t11 只填 X 必须拒绝");
        Assert(!HudMessageProtocol.TryBuildMessage(
                "t11", Fields(("yTranslation", "20")), out _, out pairError) &&
               pairError.Contains("同时", StringComparison.Ordinal),
            "t11 只填 Y 必须拒绝");
        Assert(!HudMessageProtocol.TryBuildMessage(
                "t11", Fields(("xTranslation", "NaN"), ("yTranslation", "20")),
                out _, out string numberError) && !string.IsNullOrWhiteSpace(numberError),
            "t11 非有限数字必须拒绝");

        Assert(HudMessageProtocol.ValidateRaw("  T11/10/20  ",
                out string normalized, out string validationError),
            $"合法 t11 被拒绝：{validationError}");
        AssertEqual("t11/10/20", normalized, "t11 规范化");
        Assert(HudMessageProtocol.ValidateRaw(" n-asdfg,% ", out normalized, out validationError),
            $"合法 n- 被拒绝：{validationError}");
        AssertEqual("n-asdfg,%", normalized, "n- 规范化");

        foreach (string raw in new[] { "t1\r", "t1\n", "pic-D:\\1\n2.jpg", "t1\0" })
        {
            Assert(!HudMessageProtocol.ValidateRaw(raw, out string error) &&
                   !string.IsNullOrWhiteSpace(error),
                $"包含控制字符的 raw 报文必须拒绝：{Escape(raw)}");
        }

        Assert(!HudMessageProtocol.ValidateRaw("t11/10", out validationError),
            "t11 单参数 raw 必须拒绝");
        Assert(!HudMessageProtocol.ValidateRaw("n-asdfg", out validationError),
            "n- 缺少 ,% 必须拒绝");
    }

    private static void TestResponseMatchingAndTimeouts()
    {
        AssertEqual(ResponseClassification.Success,
            HudResponseMatcher.Classify("t1", "t1_Result:1,%"), "t1 成功响应");
        AssertEqual(ResponseClassification.Success,
            HudResponseMatcher.Classify("t3", "t3_Result(25):1 2 3,%"),
            "可选点数前缀");
        AssertEqual(ResponseClassification.Success,
            HudResponseMatcher.Classify("t11/10/20", "t11/10/20_Result(25):1 2,%"),
            "带参 t11 响应关联");
        AssertEqual(ResponseClassification.Success,
            HudResponseMatcher.Classify("t21", "t3_Result(25):1 2,%"),
            "t21 兼容文档误写前缀");
        AssertEqual(ResponseClassification.Success,
            HudResponseMatcher.Classify("c-qwert", "OK%"), "辅助指令成功");

        AssertEqual(ResponseClassification.Failure,
            HudResponseMatcher.Classify("c-qwert", "Fail%"), "辅助指令失败");
        AssertEqual(ResponseClassification.Failure,
            HudResponseMatcher.Classify("t5", "Error0%"), "t5 Error0");
        AssertEqual(ResponseClassification.Failure,
            HudResponseMatcher.Classify("t5", "Error1%"), "t5 Error1");
        AssertEqual(ResponseClassification.Failure,
            HudResponseMatcher.Classify("t1", "NG%"), "通用 NG");
        AssertEqual(ResponseClassification.Failure,
            HudResponseMatcher.Classify("t1", "t1_Result:1"), "当前请求畸形终态");

        AssertEqual(ResponseClassification.Ignored,
            HudResponseMatcher.Classify("t1", "t2_Result:1,%"), "其他测试响应应忽略");
        AssertEqual(ResponseClassification.Ignored,
            HudResponseMatcher.Classify("t1", "noise%"), "无关消息应忽略");
        AssertEqual(ResponseClassification.Ignored,
            HudResponseMatcher.Classify("invalid", "OK%"), "非法请求应忽略");

        AssertEqual(TimeSpan.FromSeconds(120), HudResponseMatcher.GetTimeout("t1"),
            "光学测试超时");
        AssertEqual(TimeSpan.FromSeconds(10), HudResponseMatcher.GetTimeout("c-qwert"),
            "辅助指令超时");
        AssertEqual(HudResponseMatcher.GetTimeout("t11/10/20"),
            HudResponseMatcher.GetResponseTimeout("t11/10/20"), "超时 API 别名");
    }

    private static async Task TestTcpMessageClientAsync()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;

        await using var client = new TcpMessageClient();
        var messages = new ConcurrentQueue<string>();
        var closedEvents = new ConcurrentQueue<ConnectionClosedEventArgs>();
        using var messageSignal = new SemaphoreSlim(0);
        using var closedSignal = new SemaphoreSlim(0);
        int connectedCount = 0;

        client.Connected += (_, _) => Interlocked.Increment(ref connectedCount);
        client.MessageReceived += (_, message) =>
        {
            messages.Enqueue(message);
            messageSignal.Release();
        };
        client.ConnectionClosed += (_, args) =>
        {
            closedEvents.Enqueue(args);
            closedSignal.Release();
        };

        Task<TcpClient> acceptFirst = listener.AcceptTcpClientAsync();
        await client.ConnectAsync("127.0.0.1", port).WaitAsync(AsyncTimeout);
        using TcpClient serverFirst = await acceptFirst.WaitAsync(AsyncTimeout);
        NetworkStream firstStream = serverFirst.GetStream();

        Assert(client.IsConnected, "客户端应主动建立连接");
        AssertEqual(1, Volatile.Read(ref connectedCount), "首次 Connected 事件");
        AssertEqual("127.0.0.1", client.RemoteHost, "远端主机");
        AssertEqual(port, client.RemotePort, "远端端口");

        await client.SendAsync("t1").WaitAsync(AsyncTimeout);
        AssertEqual("t1", await ReadExactUtf8Async(firstStream, 2), "客户端发送内容");

        await WriteUtf8Async(firstStream, "t1_Result:1,");
        await Task.Delay(30);
        Assert(messages.IsEmpty, "收到 % 前不得发布半包");
        await WriteUtf8Async(firstStream, "%OK%Fail%");

        AssertEqual("t1_Result:1,%", await DequeueAsync(messages, messageSignal),
            "% 拆分半包");
        AssertEqual("OK%", await DequeueAsync(messages, messageSignal), "粘包第1帧");
        AssertEqual("Fail%", await DequeueAsync(messages, messageSignal), "粘包第2帧");

        await client.DisconnectAsync().WaitAsync(AsyncTimeout);
        ConnectionClosedEventArgs firstClosed = await DequeueAsync(closedEvents, closedSignal);
        AssertEqual(ConnectionClosedReason.LocalDisconnect, firstClosed.Reason,
            "主动断开原因");
        Assert(!client.IsConnected, "主动断开后 IsConnected=false");

        // 旧服务端即使还能把字节交给本机 TCP 缓冲，也不能污染下一代客户端会话。
        try
        {
            await WriteUtf8Async(firstStream, "stale%");
        }
        catch (IOException)
        {
            // 对端已观察到主动断开，同样是预期结果。
        }
        catch (SocketException)
        {
            // 同上。
        }

        Task<TcpClient> acceptSecond = listener.AcceptTcpClientAsync();
        await client.ConnectAsync("127.0.0.1", port).WaitAsync(AsyncTimeout);
        using TcpClient serverSecond = await acceptSecond.WaitAsync(AsyncTimeout);
        NetworkStream secondStream = serverSecond.GetStream();
        AssertEqual(2, Volatile.Read(ref connectedCount), "重连 Connected 事件");
        Assert(client.IsConnected, "重连后应处于连接状态");

        await client.SendAsync("t2").WaitAsync(AsyncTimeout);
        AssertEqual("t2", await ReadExactUtf8Async(secondStream, 2), "重连后的发送内容");
        await WriteUtf8Async(secondStream, "fresh%");
        AssertEqual("fresh%", await DequeueAsync(messages, messageSignal),
            "重连只接收新会话消息");
        await Task.Delay(50);
        Assert(messages.IsEmpty, "旧会话不得在重连后发布消息");

        await client.DisconnectAsync().WaitAsync(AsyncTimeout);
        ConnectionClosedEventArgs secondClosed = await DequeueAsync(closedEvents, closedSignal);
        AssertEqual(ConnectionClosedReason.LocalDisconnect, secondClosed.Reason,
            "重连后的主动断开原因");
        Assert(secondClosed.ConnectionGeneration > firstClosed.ConnectionGeneration,
            "重连必须使用更高的连接代次");
        Assert(closedEvents.IsEmpty, "每代主动断开只应发布一次关闭事件");
    }

    private static void TestMainFormOnStaThread()
    {
        Exception? failure = null;
        var thread = new Thread(() =>
        {
            try
            {
                using var form = new MainForm();

                TextBox host = GetPrivateField<TextBox>(form, "txtHost");
                NumericUpDown port = GetPrivateField<NumericUpDown>(form, "numPort");
                TabControl tabs = GetPrivateField<TabControl>(form, "mainTabs");
                DataGridView commands = GetPrivateField<DataGridView>(form, "dgvCommands");
                DataGridView responseFields =
                    GetPrivateField<DataGridView>(form, "dgvResponseFields");
                TextBox rawMessage = GetPrivateField<TextBox>(form, "txtRawMessage");
                Label responseHint = GetPrivateField<Label>(form, "lblResponseHint");
                TextBox imageDirectory = GetPrivateField<TextBox>(form, "txtImageDirectory");
                ComboBox topology = GetPrivateField<ComboBox>(form, "cmbTopology");
                NumericUpDown projectionInterval =
                    GetPrivateField<NumericUpDown>(form, "numProjectionIntervalSeconds");
                CheckBox restoreWallpaper =
                    GetPrivateField<CheckBox>(form, "chkRestoreWallpaper");

                AssertEqual("127.0.0.1", host.Text, "默认服务器地址");
                Assert(host.Enabled && !host.ReadOnly, "顶部服务器地址应可编辑");
                AssertEqual(5556m, port.Value, "默认服务器端口");
                Assert(port.Enabled, "顶部端口应可编辑");

                AssertEqual(3, tabs.TabPages.Count, "主界面页签数");
                AssertSequenceEqual(["投影与批量测试", "协议命令编辑", "通信监视"],
                    tabs.TabPages.Cast<TabPage>().Select(page => page.Text), "主界面页签标题");

                DataGridViewRow[] commandRows = commands.Rows.Cast<DataGridViewRow>()
                    .Where(row => !row.IsNewRow).ToArray();
                AssertEqual(26, commandRows.Length, "协议列表行数");
                AssertSequenceEqual(HudCommandCatalog.All.Select(item => item.Code),
                    commandRows.Select(row => Convert.ToString(row.Cells[1].Value) ?? string.Empty),
                    "协议列表命令顺序");
                AssertEqual("t1", rawMessage.Text, "初始选中命令的原始报文");
                Assert(!rawMessage.ReadOnly, "完整发送内容应可编辑");
                AssertEqual(18, responseFields.Rows.Cast<DataGridViewRow>()
                    .Count(row => !row.IsNewRow), "t1 返回字段对照行数");

                rawMessage.Text = "t2";
                Application.DoEvents();
                Assert(rawMessage.BackColor != Color.White,
                    "原始报文改成其他命令时应显示校验错误");
                Assert(responseHint.Text.Contains("发送内容属于 t2", StringComparison.Ordinal),
                    "错配提示应指出实际命令");
                MethodInfo tryGetCurrentMessage = form.GetType().GetMethod(
                    "TryGetCurrentMessage",
                    BindingFlags.Instance | BindingFlags.NonPublic)!;
                object?[] messageArguments = [null, false];
                bool mismatchedMessageAccepted = (bool)tryGetCurrentMessage.Invoke(
                    form, messageArguments)!;
                Assert(!mismatchedMessageAccepted,
                    "不能在显示 t1 含义时发送合法但错配的 t2 报文");
                rawMessage.Text = "t1";
                Application.DoEvents();

                Assert(imageDirectory.Enabled && !imageDirectory.ReadOnly,
                    "投影图片目录应可编辑");
                AssertEqual(4, topology.SelectedIndex, "默认投影模式应为扩展屏幕");
                AssertEqual(5m, projectionInterval.Value, "默认投图间隔");
                Assert(restoreWallpaper.Checked, "默认应在停止后恢复桌面");

                AssertSequenceEqual([2, 3, 0, 1], MainForm.BuildImageTestOrder(2, 4),
                    "投影批量顺序应从选中项循环一周");
                AssertSequenceEqual([0], MainForm.BuildImageTestOrder(0, 1),
                    "单图投影顺序");
                AssertThrows<ArgumentOutOfRangeException>(
                    () => MainForm.BuildImageTestOrder(0, 0), "零图片数量");
                AssertThrows<ArgumentOutOfRangeException>(
                    () => MainForm.BuildImageTestOrder(4, 4), "越界起始下标");
            }
            catch (Exception ex)
            {
                failure = ex;
            }
        })
        {
            IsBackground = true,
            Name = "MainForm smoke test"
        };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        if (!thread.Join(AsyncTimeout))
        {
            throw new TimeoutException("STA MainForm 烟雾测试超时。");
        }

        if (failure is not null)
        {
            throw new InvalidOperationException("STA MainForm 烟雾测试失败。", failure);
        }
    }

    private static Dictionary<string, string?> Fields(params (string Key, string? Value)[] values) =>
        values.ToDictionary(item => item.Key, item => item.Value, StringComparer.OrdinalIgnoreCase);

    /// <summary>离屏渲染三个页签，供人工检查 DPI、截断和布局问题。</summary>
    private static void RenderUiSnapshots(string outputDirectory, Size formSize)
    {
        Directory.CreateDirectory(outputDirectory);
        using var form = new MainForm
        {
            Size = formSize,
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-20000, -20000),
            ShowInTaskbar = false
        };
        // 句柄必须经历一次 Show 才能让 TabPage 子树完成真实布局；窗口放在屏幕外，不打扰桌面。
        form.Show();
        Application.DoEvents();
        TabControl tabs = GetPrivateField<TabControl>(form, "mainTabs");
        string[] names = ["projection", "protocol", "communication"];

        for (int index = 0; index < tabs.TabPages.Count; index++)
        {
            tabs.SelectedIndex = index;
            if (index == 1)
            {
                // t11 同时覆盖“无参数”和 X/Y 成对字段，是协议页最有代表性的视觉样例。
                DataGridView commands = GetPrivateField<DataGridView>(form, "dgvCommands");
                commands.ClearSelection();
                commands.Rows[10].Selected = true;
                commands.CurrentCell = commands.Rows[10].Cells[1];
                form.GetType().GetMethod(
                    "ShowSelectedDefinition",
                    BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(form, null);
            }
            form.PerformLayout();
            tabs.PerformLayout();
            form.Refresh();
            Application.DoEvents();

            using var bitmap = new Bitmap(form.ClientSize.Width, form.ClientSize.Height);
            form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.ClientSize));
            bitmap.Save(
                Path.Combine(outputDirectory, $"{index + 1}-{names[index]}.png"),
                System.Drawing.Imaging.ImageFormat.Png);
        }
        form.Hide();
    }

    private static async Task<string> ReadExactUtf8Async(NetworkStream stream, int byteCount)
    {
        byte[] buffer = new byte[byteCount];
        int offset = 0;
        using var timeout = new CancellationTokenSource(AsyncTimeout);
        while (offset < buffer.Length)
        {
            int read = await stream.ReadAsync(buffer.AsMemory(offset), timeout.Token);
            if (read == 0)
            {
                throw new EndOfStreamException("服务端在收到完整客户端请求前断开。");
            }
            offset += read;
        }
        return Encoding.UTF8.GetString(buffer);
    }

    private static Task WriteUtf8Async(NetworkStream stream, string value) =>
        stream.WriteAsync(Encoding.UTF8.GetBytes(value)).AsTask();

    private static async Task<T> DequeueAsync<T>(
        ConcurrentQueue<T> queue,
        SemaphoreSlim signal)
    {
        if (!await signal.WaitAsync(AsyncTimeout))
        {
            throw new TimeoutException("等待异步事件超时。");
        }
        return queue.TryDequeue(out T? value)
            ? value
            : throw new InvalidOperationException("事件信号与队列状态不一致。");
    }

    private static T GetPrivateField<T>(object target, string name) where T : class
    {
        FieldInfo field = target.GetType().GetField(
            name, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new MissingFieldException(target.GetType().FullName, name);
        return field.GetValue(target) as T
            ?? throw new InvalidCastException($"字段 {name} 不是 {typeof(T).Name}。");
    }

    private static string Escape(string value) =>
        value.Replace("\r", "\\r", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal)
            .Replace("\0", "\\0", StringComparison.Ordinal);

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void AssertEqual<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException(
                $"{message}：期望 {Format(expected)}，实际 {Format(actual)}。");
        }
    }

    private static void AssertSequenceEqual<T>(
        IEnumerable<T> expected,
        IEnumerable<T> actual,
        string message)
    {
        T[] expectedArray = expected.ToArray();
        T[] actualArray = actual.ToArray();
        if (!expectedArray.SequenceEqual(actualArray))
        {
            throw new InvalidOperationException(
                $"{message}：期望 [{string.Join(", ", expectedArray)}]，" +
                $"实际 [{string.Join(", ", actualArray)}]。");
        }
    }

    private static void AssertThrows<TException>(Action action, string message)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            return;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"{message}：期望 {typeof(TException).Name}，实际 {ex.GetType().Name}。", ex);
        }
        throw new InvalidOperationException($"{message}：未抛出 {typeof(TException).Name}。");
    }

    private static string Format<T>(T value) => value?.ToString() ?? "<null>";
}
