using HudOpticalTestClient.Networking;
using HudOpticalTestClient.Projection;
using HudOpticalTestClient.Protocol;

namespace HudOpticalTestClient;

/// <summary>
/// HUD 光学测试工作台。原项目的投影子界面在这里成为主界面，
/// TCP 客户端连接和协议指令编辑作为两个紧凑的辅助区域加入。
/// </summary>
public partial class MainForm : Form
{
    private static readonly HashSet<string> SupportedExtensions =
        new(StringComparer.OrdinalIgnoreCase) { ".png", ".bmp", ".jpg", ".jpeg" };

    private readonly IDesktopDisplayService _desktopDisplay = new DesktopDisplayService();
    private readonly TcpMessageClient _tcpClient = new();
    private readonly CancellationTokenSource _formCancellation = new();
    private readonly Dictionary<string, string> _messageDrafts =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, Dictionary<string, string?>> _fieldDrafts =
        new(StringComparer.OrdinalIgnoreCase);

    private List<string> _imageFiles = [];
    private CancellationTokenSource? _batchCancellation;
    private string? _frozenBatchMessage;
    private TestDefinition? _selectedDefinition;
    private int _currentImageIndex = -1;
    private string? _originalWallpaper;
    private bool _formCancellationDisposed;
    private bool _isBatchRunning;
    private bool _isConnecting;
    private bool _isClosing;
    private bool _isSending;
    private bool _isTimedProjectionRunning;
    private bool _updatingProtocolUi;

    /// <summary>初始化可视化协议目录，并订阅客户端连接事件。</summary>
    public MainForm()
    {
        InitializeComponent();
        if (cmbTopology.Items.Count > 4) cmbTopology.SelectedIndex = 4;

        _tcpClient.Connected += TcpClient_Connected;
        _tcpClient.MessageReceived += TcpClient_MessageReceived;
        _tcpClient.ConnectionClosed += TcpClient_ConnectionClosed;

        InitializeProtocolEditor();
        SetConnectionUi("未连接");
    }

    /// <summary>首次显示时加载默认图片目录；连接动作由用户显式触发。</summary>
    private void MainForm_Load(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtImageDirectory.Text))
        {
            txtImageDirectory.Text =
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        }

        RefreshImageList(showError: false);
    }

    #region TCP 客户端

    /// <summary>连接或主动断开光学软件服务器。</summary>
    private async void btnConnect_Click(object? sender, EventArgs e)
    {
        if (_isConnecting) return;

        if (_tcpClient.IsConnected)
        {
            btnConnect.Enabled = false;
            _batchCancellation?.Cancel();
            try
            {
                await _tcpClient.DisconnectAsync(_formCancellation.Token);
            }
            catch (OperationCanceledException) when (_isClosing)
            {
                // 窗口关闭时不再显示提示。
            }
            finally
            {
                if (!_isClosing) SetConnectionUi("未连接");
            }
            return;
        }

        string host = txtHost.Text.Trim();
        if (host.Length == 0)
        {
            MessageBox.Show(this, "请输入光学软件服务器地址。", "连接参数",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtHost.Focus();
            return;
        }

        int port = decimal.ToInt32(numPort.Value);
        _isConnecting = true;
        SetConnectionUi("连接中...");
        AppendTcpLog("系统", $"正在连接 {host}:{port}");

        using var connectCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            _formCancellation.Token);
        connectCancellation.CancelAfter(TimeSpan.FromSeconds(10));
        try
        {
            await _tcpClient.ConnectAsync(host, port, connectCancellation.Token);
        }
        catch (OperationCanceledException) when (_isClosing)
        {
            // 窗口关闭会取消连接，不弹出错误。
        }
        catch (OperationCanceledException)
        {
            const string error = "连接服务器超时（10 秒），请检查地址、端口和光学软件状态。";
            AppendTcpLog("错误", error);
            MessageBox.Show(this, error, "连接超时",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            if (!_isClosing)
            {
                AppendTcpLog("错误", $"连接失败：{ex.Message}");
                MessageBox.Show(this, ex.Message, "连接光学软件失败",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _isConnecting = false;
            if (!_isClosing)
            {
                SetConnectionUi(_tcpClient.IsConnected ? "已连接" : "未连接");
            }
        }
    }

    private void TcpClient_Connected(object? sender, EventArgs e) =>
        PostToUi(() =>
        {
            AppendTcpLog("系统",
                $"已连接 {_tcpClient.RemoteHost}:{_tcpClient.RemotePort}");
            SetConnectionUi("已连接");
        });

    private void TcpClient_MessageReceived(object? sender, string message) =>
        PostToUi(() => AppendTcpLog("接收", message));

    private void TcpClient_ConnectionClosed(
        object? sender,
        ConnectionClosedEventArgs e)
    {
        // 先在网络线程同步发出取消，再排队更新界面，消除 UI 消息队列造成的延迟窗口。
        _batchCancellation?.Cancel();
        PostToUi(() =>
        {
            AppendTcpLog("系统", e.Description);
            if (!_isClosing) SetConnectionUi("未连接");
        });
    }

    private void SetConnectionUi(string stateText)
    {
        bool connected = _tcpClient.IsConnected;
        bool inputsEnabled = !_isConnecting && !connected;

        txtHost.Enabled = inputsEnabled;
        numPort.Enabled = inputsEnabled;
        btnConnect.Enabled = !_isConnecting;
        btnConnect.Text = connected ? "断开" : _isConnecting ? "连接中..." : "连接";
        lblConnectionStatus.Text = connected ? $"● {stateText}" : $"○ {stateText}";
        lblConnectionStatus.ForeColor = connected
            ? Color.FromArgb(21, 94, 61)
            : _isConnecting
                ? Color.FromArgb(30, 64, 175)
                : Color.FromArgb(71, 85, 105);
        lblConnectionStatus.BackColor = connected
            ? Color.FromArgb(220, 252, 231)
            : _isConnecting
                ? Color.FromArgb(219, 234, 254)
                : Color.FromArgb(241, 245, 249);

        UpdateActionAvailability();
    }

    private bool EnsureConnected(bool showMessage = true)
    {
        if (_tcpClient.IsConnected) return true;

        if (showMessage)
        {
            MessageBox.Show(this, "请先连接光学软件服务器。", "尚未连接",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            txtHost.Focus();
        }
        return false;
    }

    private void AppendTcpLog(string direction, string message)
    {
        txtTcpLog.AppendText(
            $"[{DateTime.Now:HH:mm:ss}] [{direction}] {message}{Environment.NewLine}");
    }

    private void btnClearTcpLog_Click(object? sender, EventArgs e) => txtTcpLog.Clear();

    #endregion

    #region 协议目录与可视编辑

    private void InitializeProtocolEditor()
    {
        _updatingProtocolUi = true;
        try
        {
            dgvCommands.Rows.Clear();
            foreach (TestDefinition definition in HudCommandCatalog.All)
            {
                var values = definition.Fields.ToDictionary(
                    field => field.Key,
                    field => (string?)field.DefaultValue,
                    StringComparer.OrdinalIgnoreCase);
                _fieldDrafts[definition.Code] = values;

                string message = definition.DefaultMessage;
                if (HudMessageProtocol.TryBuildMessage(
                        definition.Code, values, out string built, out _))
                {
                    message = built;
                }
                _messageDrafts[definition.Code] = message;

                int rowIndex = dgvCommands.Rows.Add(
                    definition.Category,
                    definition.Code,
                    definition.Name,
                    message);
                dgvCommands.Rows[rowIndex].Tag = definition;
            }

            if (dgvCommands.Rows.Count > 0)
            {
                dgvCommands.ClearSelection();
                dgvCommands.Rows[0].Selected = true;
                dgvCommands.CurrentCell = dgvCommands.Rows[0].Cells[colCommandCode.Index];
            }
        }
        finally
        {
            _updatingProtocolUi = false;
        }

        ShowSelectedDefinition();
    }

    private void dgvCommands_SelectionChanged(object? sender, EventArgs e)
    {
        if (!_updatingProtocolUi) ShowSelectedDefinition();
    }

    private void ShowSelectedDefinition()
    {
        if (dgvCommands.CurrentRow?.Tag is not TestDefinition definition) return;

        _selectedDefinition = definition;
        _updatingProtocolUi = true;
        try
        {
            lblSelectedCommandTitle.Text =
                $"{definition.Code}  ·  {definition.Name}";
            txtCommandDescription.Text = definition.Description;

            dgvFields.Rows.Clear();
            Dictionary<string, string?> values = _fieldDrafts[definition.Code];
            foreach (ProtocolFieldDefinition field in definition.Fields)
            {
                string unit = field.Unit.Length == 0 ? string.Empty : $"；单位：{field.Unit}";
                string example = field.Example.Length == 0 ? string.Empty : $"；示例：{field.Example}";
                string required = field.Required ? "；必填" : "；可选";
                int index = dgvFields.Rows.Add(
                    field.Name,
                    $"{field.Description}{unit}{example}{required}",
                    values.GetValueOrDefault(field.Key) ?? string.Empty);
                dgvFields.Rows[index].Tag = field;
            }

            dgvResponseFields.Rows.Clear();
            for (int index = 0; index < definition.ResponseFields.Count; index++)
            {
                ProtocolFieldDefinition field = definition.ResponseFields[index];
                string repeated = field.IsRepeated ? "（重复组）" : string.Empty;
                string unit = field.Unit.Length == 0 ? string.Empty : $" [{field.Unit}]";
                string description = field.Description.Length == 0
                    ? field.Name
                    : $"{field.Name} — {field.Description}";
                dgvResponseFields.Rows.Add(
                    (index + 1).ToString(),
                    $"{description}{unit}{repeated}");
            }

            txtRawMessage.Text = _messageDrafts[definition.Code];
            SetResponseHint();
        }
        finally
        {
            _updatingProtocolUi = false;
        }

        bool fieldsSynchronized = TrySynchronizeFieldsFromRaw(
            definition, txtRawMessage.Text, updateGrid: true);
        ValidateRawDraft(fieldsSynchronized ? null : "原始内容无法回填字段表；字段编辑已锁定。可继续直接发送原始内容，或恢复默认值。");
        UpdateBatchCommandLabel();
    }

    private void dgvCommands_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        if (_updatingProtocolUi || e.RowIndex < 0 ||
            e.ColumnIndex != colCommandMessage.Index) return;
        if (dgvCommands.Rows[e.RowIndex].Tag is not TestDefinition definition) return;

        string message = Convert.ToString(
            dgvCommands.Rows[e.RowIndex].Cells[colCommandMessage.Index].Value) ?? string.Empty;
        _messageDrafts[definition.Code] = message;

        if (ReferenceEquals(_selectedDefinition, definition))
        {
            _updatingProtocolUi = true;
            txtRawMessage.Text = message;
            _updatingProtocolUi = false;
            bool fieldsSynchronized = TrySynchronizeFieldsFromRaw(
                definition, message, updateGrid: true);
            ValidateRawDraft(fieldsSynchronized ? null : "原始内容无法回填字段表；字段编辑已锁定。可继续直接发送原始内容，或恢复默认值。");
            UpdateBatchCommandLabel();
        }
    }

    private void dgvFields_CellEndEdit(object? sender, DataGridViewCellEventArgs e)
    {
        if (_updatingProtocolUi || _selectedDefinition is null || e.RowIndex < 0 ||
            e.ColumnIndex != colFieldValue.Index) return;
        if (dgvFields.Rows[e.RowIndex].Tag is not ProtocolFieldDefinition field) return;

        Dictionary<string, string?> values = _fieldDrafts[_selectedDefinition.Code];
        values[field.Key] = Convert.ToString(
            dgvFields.Rows[e.RowIndex].Cells[colFieldValue.Index].Value) ?? string.Empty;

        if (HudMessageProtocol.TryBuildMessage(
                _selectedDefinition.Code,
                values,
                out string message,
                out string error))
        {
            SetCurrentDraft(message);
            SetResponseHint();
        }
        else
        {
            SetResponseHint(error);
        }
    }

    private void txtRawMessage_TextChanged(object? sender, EventArgs e)
    {
        if (_updatingProtocolUi || _selectedDefinition is null) return;

        _messageDrafts[_selectedDefinition.Code] = txtRawMessage.Text;
        if (dgvCommands.CurrentRow is not null)
        {
            _updatingProtocolUi = true;
            dgvCommands.CurrentRow.Cells[colCommandMessage.Index].Value = txtRawMessage.Text;
            _updatingProtocolUi = false;
        }

        bool fieldsSynchronized = TrySynchronizeFieldsFromRaw(
            _selectedDefinition, txtRawMessage.Text, updateGrid: true);
        ValidateRawDraft(fieldsSynchronized ? null : "原始内容无法回填字段表；字段编辑已锁定。可继续直接发送原始内容，或恢复默认值。");
        UpdateBatchCommandLabel();
    }

    private void btnRestoreDefault_Click(object? sender, EventArgs e)
    {
        if (_selectedDefinition is null) return;

        Dictionary<string, string?> values = _fieldDrafts[_selectedDefinition.Code];
        values.Clear();
        foreach (ProtocolFieldDefinition field in _selectedDefinition.Fields)
        {
            values[field.Key] = field.DefaultValue;
        }

        _messageDrafts[_selectedDefinition.Code] = _selectedDefinition.DefaultMessage;
        ShowSelectedDefinition();
    }

    private void SetCurrentDraft(string message)
    {
        if (_selectedDefinition is null) return;

        _messageDrafts[_selectedDefinition.Code] = message;
        _updatingProtocolUi = true;
        try
        {
            txtRawMessage.Text = message;
            if (dgvCommands.CurrentRow is not null)
            {
                dgvCommands.CurrentRow.Cells[colCommandMessage.Index].Value = message;
            }
        }
        finally
        {
            _updatingProtocolUi = false;
        }
        TrySynchronizeFieldsFromRaw(_selectedDefinition, message, updateGrid: true);
        UpdateBatchCommandLabel();
    }

    /// <summary>
    /// 将可识别的原始指令反解析回动态字段。解析不了时保留原始文本，
    /// 但锁定字段表，避免下一次字段编辑用旧值覆盖用户的高级手工改动。
    /// </summary>
    private bool TrySynchronizeFieldsFromRaw(
        TestDefinition definition,
        string raw,
        bool updateGrid)
    {
        if (definition.Fields.Count == 0)
        {
            dgvFields.Enabled = false;
            return true;
        }

        var parsed = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        bool success = definition.Code switch
        {
            "t11" => TryParseT11(raw, parsed),
            "c-" => TryParseSingleValue(raw, "c-", suffix: string.Empty, "recipe", parsed),
            "pic-" => TryParseSingleValue(raw, "pic-", suffix: string.Empty, "filePath", parsed),
            "bmp-" => TryParseBmp(raw, parsed),
            "n-" => TryParseSingleValue(raw, "n-", suffix: ",%", "workbookName", parsed),
            _ => false
        };

        dgvFields.Enabled = success && !_isBatchRunning;
        if (!success) return false;

        Dictionary<string, string?> values = _fieldDrafts[definition.Code];
        foreach (ProtocolFieldDefinition field in definition.Fields)
        {
            values[field.Key] = parsed.GetValueOrDefault(field.Key) ?? string.Empty;
        }

        if (updateGrid)
        {
            _updatingProtocolUi = true;
            try
            {
                foreach (DataGridViewRow row in dgvFields.Rows)
                {
                    if (row.Tag is ProtocolFieldDefinition field)
                    {
                        row.Cells[colFieldValue.Index].Value =
                            values.GetValueOrDefault(field.Key) ?? string.Empty;
                    }
                }
            }
            finally
            {
                _updatingProtocolUi = false;
            }
        }
        return true;
    }

    private static bool TryParseT11(
        string raw,
        IDictionary<string, string?> values)
    {
        string message = raw.Trim();
        if (message.Equals("t11", StringComparison.OrdinalIgnoreCase))
        {
            values["xTranslation"] = string.Empty;
            values["yTranslation"] = string.Empty;
            return true;
        }

        string[] parts = message.Split('/');
        if (parts.Length != 3 ||
            !parts[0].Equals("t11", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(parts[1]) ||
            string.IsNullOrWhiteSpace(parts[2])) return false;

        values["xTranslation"] = parts[1].Trim();
        values["yTranslation"] = parts[2].Trim();
        return true;
    }

    private static bool TryParseSingleValue(
        string raw,
        string prefix,
        string suffix,
        string key,
        IDictionary<string, string?> values)
    {
        string message = raw.Trim();
        if (!message.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
            !message.EndsWith(suffix, StringComparison.Ordinal) ||
            message.Length <= prefix.Length + suffix.Length) return false;

        int length = message.Length - prefix.Length - suffix.Length;
        string value = message.Substring(prefix.Length, length).Trim();
        if (value.Length == 0) return false;
        values[key] = value;
        return true;
    }

    private static bool TryParseBmp(
        string raw,
        IDictionary<string, string?> values)
    {
        string message = raw.Trim();
        if (!message.StartsWith("bmp-", StringComparison.OrdinalIgnoreCase)) return false;

        string body = message[4..];
        int separator = body.IndexOf('|');
        if (separator <= 0 || separator != body.LastIndexOf('|') ||
            separator == body.Length - 1) return false;

        values["directory"] = body[..separator].Trim();
        values["baseName"] = body[(separator + 1)..].Trim();
        return values["directory"]!.Length > 0 && values["baseName"]!.Length > 0;
    }

    private void ValidateRawDraft(string? synchronizationError = null)
    {
        bool valid = HudMessageProtocol.ValidateRaw(
            txtRawMessage.Text,
            out string normalizedMessage,
            out string error);
        if (valid)
        {
            valid = TryEnsureMessageMatchesSelection(normalizedMessage, out error);
        }

        if (valid && string.IsNullOrWhiteSpace(synchronizationError))
        {
            txtRawMessage.BackColor = Color.White;
            SetResponseHint();
        }
        else
        {
            txtRawMessage.BackColor = string.IsNullOrWhiteSpace(error)
                ? Color.FromArgb(255, 251, 235)
                : Color.FromArgb(254, 242, 242);
            SetResponseHint(string.IsNullOrWhiteSpace(error) ? synchronizationError : error);
        }
    }

    private bool TryEnsureMessageMatchesSelection(string message, out string error)
    {
        if (_selectedDefinition is null)
        {
            error = "请先从左侧命令目录选择一个测试项目。";
            return false;
        }

        if (!HudMessageProtocol.TryGetDefinition(message, out TestDefinition actualDefinition))
        {
            error = "无法识别发送内容所属的 HUD 命令。";
            return false;
        }

        if (actualDefinition.Code.Equals(
                _selectedDefinition.Code,
                StringComparison.OrdinalIgnoreCase))
        {
            error = string.Empty;
            return true;
        }

        error = $"发送内容属于 {actualDefinition.Code}，但当前含义对照项是 {_selectedDefinition.Code}。" +
                $"请在左侧命令目录选择 {actualDefinition.Code}，或恢复 {_selectedDefinition.Code} 的默认发送内容。";
        return false;
    }

    private void SetResponseHint(string? error = null)
    {
        if (_selectedDefinition is null) return;

        lblResponseHint.Text = string.IsNullOrWhiteSpace(error)
            ? $"返回参考：{_selectedDefinition.ResponseHint}"
            : $"编辑检查：{error}    |    返回参考：{_selectedDefinition.ResponseHint}";
        lblResponseHint.ForeColor = string.IsNullOrWhiteSpace(error)
            ? Color.FromArgb(71, 85, 105)
            : Color.FromArgb(185, 28, 28);
    }

    private bool TryGetCurrentMessage(out string message, bool showError)
    {
        bool valid = HudMessageProtocol.ValidateRaw(
            txtRawMessage.Text,
            out message,
            out string error);
        if (valid)
        {
            valid = TryEnsureMessageMatchesSelection(message, out error);
        }

        if (!valid)
        {
            if (showError)
            {
                MessageBox.Show(this, error, "发送内容检查",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                mainTabs.SelectedTab = tabProtocol;
                txtRawMessage.Focus();
            }
            return false;
        }
        return true;
    }

    private async void btnSendOnce_Click(object? sender, EventArgs e)
    {
        if (_isSending || _isBatchRunning) return;
        if (!TryGetCurrentMessage(out string message, showError: true) ||
            !EnsureConnected()) return;

        _isSending = true;
        UpdateActionAvailability();
        try
        {
            await _tcpClient.SendAsync(message, _formCancellation.Token);
            AppendTcpLog("发送", message);
        }
        catch (OperationCanceledException) when (_isClosing)
        {
            // 窗口关闭时忽略。
        }
        catch (Exception ex)
        {
            if (!_isClosing)
            {
                AppendTcpLog("错误", $"发送失败：{ex.Message}");
                MessageBox.Show(this, ex.Message, "发送失败",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _isSending = false;
            if (!_isClosing) UpdateActionAvailability();
        }
    }

    private void btnChooseCommand_Click(object? sender, EventArgs e) =>
        mainTabs.SelectedTab = tabProtocol;

    private void UpdateBatchCommandLabel()
    {
        if (_selectedDefinition is null)
        {
            lblBatchCommand.Text = "当前联测指令：未选择";
            return;
        }

        string message = _messageDrafts.GetValueOrDefault(_selectedDefinition.Code) ?? string.Empty;
        lblBatchCommand.Text = _isBatchRunning && _frozenBatchMessage is not null
            ? $"本批次已冻结：{_frozenBatchMessage}（编辑不会影响正在运行的批次）"
            : $"当前联测指令：{_selectedDefinition.Code} · {_selectedDefinition.Name}  →  {message}";
    }

    #endregion

    #region 图片与投影

    private void btnBrowseDirectory_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "选择包含投影图片的目录",
            SelectedPath = Directory.Exists(txtImageDirectory.Text)
                ? txtImageDirectory.Text
                : string.Empty,
            ShowNewFolderButton = false
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            txtImageDirectory.Text = dialog.SelectedPath;
            RefreshImageList(showError: true);
        }
    }

    private void btnRefreshImages_Click(object? sender, EventArgs e) =>
        RefreshImageList(showError: true);

    private void btnProjectNext_Click(object? sender, EventArgs e) =>
        ProjectNextImage(showError: true);

    private void btnProjectSelected_Click(object? sender, EventArgs e)
    {
        if (_imageFiles.Count == 0)
        {
            RefreshImageList(showError: true);
            if (_imageFiles.Count == 0) return;
        }

        if (lvImages.SelectedIndices.Count != 1)
        {
            MessageBox.Show(this, "请先在图片列表中选择一张图片。", "未选择图片",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        ProjectImageAtIndex(lvImages.SelectedIndices[0], showError: true);
    }

    private bool ProjectNextImage(bool showError)
    {
        if (_imageFiles.Count == 0)
        {
            RefreshImageList(showError);
            if (_imageFiles.Count == 0) return false;
        }

        int nextIndex = (_currentImageIndex + 1) % _imageFiles.Count;
        return ProjectImageAtIndex(nextIndex, showError);
    }

    private bool ProjectImageAtIndex(
        int imageIndex,
        bool showError,
        CancellationToken cancellationToken = default)
    {
        if (imageIndex < 0 || imageIndex >= _imageFiles.Count) return false;

        string imagePath = _imageFiles[imageIndex];
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            CaptureOriginalWallpaper();
            // 批量联测的断线事件会同步取消 Token；在不可逆的壁纸切换前再做一次检查。
            cancellationToken.ThrowIfCancellationRequested();
            _desktopDisplay.SetWallpaper(imagePath);

            DisplayTopology topology = GetSelectedTopology();
            if (topology != DisplayTopology.None)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _desktopDisplay.ApplyTopology(topology);
            }

            ShowPreview(imagePath);
            _currentImageIndex = imageIndex;
            ListViewItem item = lvImages.Items[imageIndex];
            item.SubItems[2].Text = "已投图";
            item.Selected = true;
            item.EnsureVisible();
            lblCurrentImage.Text = $"当前图片：{Path.GetFileName(imagePath)}";
            lblProjectionState.Text = $"已投 {imageIndex + 1}/{_imageFiles.Count}";
            AppendLog($"投图成功：{Path.GetFileName(imagePath)}（{cmbTopology.Text}）");
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            lblProjectionState.Text = "投图失败";
            AppendLog($"投图失败：{ex.Message}");
            if (showError)
            {
                MessageBox.Show(this, ex.Message, "投图失败",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }
    }

    private void btnTimedProjection_Click(object? sender, EventArgs e)
    {
        if (_isTimedProjectionRunning)
        {
            StopTimedProjection("定时投图已停止。", restoreWallpaper: true);
            return;
        }

        if (_imageFiles.Count == 0)
        {
            RefreshImageList(showError: true);
            if (_imageFiles.Count == 0) return;
        }

        projectionTimer.Interval = checked(
            decimal.ToInt32(numProjectionIntervalSeconds.Value * 1000m));
        SetTimedProjectionState(true);
        AppendLog(
            $"开始定时投图：共 {_imageFiles.Count} 张，间隔 {numProjectionIntervalSeconds.Value} 秒。");

        if (!ProjectNextImage(showError: true))
        {
            StopTimedProjection("定时投图因切图失败而停止。", restoreWallpaper: true);
            return;
        }
        projectionTimer.Start();
    }

    private void projectionTimer_Tick(object? sender, EventArgs e)
    {
        if (!ProjectNextImage(showError: false))
        {
            StopTimedProjection("定时投图因切图失败而停止。", restoreWallpaper: true);
        }
    }

    private void StopTimedProjection(string? logMessage, bool restoreWallpaper)
    {
        projectionTimer.Stop();
        SetTimedProjectionState(false);
        if (!string.IsNullOrEmpty(logMessage)) AppendLog(logMessage);
        if (restoreWallpaper && chkRestoreWallpaper.Checked)
        {
            RestoreOriginalWallpaper(showError: false);
        }
    }

    private void SetTimedProjectionState(bool running)
    {
        _isTimedProjectionRunning = running;
        grpSettings.Enabled = !_isBatchRunning;
        bool settingsEnabled = !running && !_isBatchRunning;
        txtImageDirectory.Enabled = settingsEnabled;
        btnBrowseDirectory.Enabled = settingsEnabled;
        cmbTopology.Enabled = settingsEnabled;
        btnApplyTopology.Enabled = settingsEnabled;
        numProjectionIntervalSeconds.Enabled = settingsEnabled;
        chkRestoreWallpaper.Enabled = settingsEnabled;
        btnTimedProjection.Enabled = !_isBatchRunning;
        btnRefreshImages.Enabled = settingsEnabled;
        btnProjectNext.Enabled = settingsEnabled;
        btnProjectSelected.Enabled = settingsEnabled;
        lvImages.Enabled = settingsEnabled;
        btnTimedProjection.Text = running ? "停止定时投图" : "开始定时投图";
        lblProjectionState.Text = running ? "定时投图中" : "未启动定时投图";
        UpdateActionAvailability();
    }

    private void btnApplyTopology_Click(object? sender, EventArgs e)
    {
        try
        {
            DisplayTopology topology = GetSelectedTopology();
            if (topology == DisplayTopology.None)
            {
                MessageBox.Show(this, "请选择需要应用的投影模式。", "投影模式",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _desktopDisplay.ApplyTopology(topology);
            AppendLog($"已应用投影模式：{cmbTopology.Text}");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "切换投影模式失败",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private DisplayTopology GetSelectedTopology() => cmbTopology.SelectedIndex switch
    {
        1 => DisplayTopology.Internal,
        2 => DisplayTopology.Clone,
        3 => DisplayTopology.External,
        4 => DisplayTopology.Extend,
        _ => DisplayTopology.None
    };

    private bool RefreshImageList(bool showError)
    {
        string directory = txtImageDirectory.Text.Trim();
        if (!Directory.Exists(directory))
        {
            _imageFiles = [];
            lvImages.Items.Clear();
            lblProjectionState.Text = "图片目录不存在";
            if (showError)
            {
                MessageBox.Show(this, "图片目录不存在，请重新选择。", "图片目录",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            return false;
        }

        try
        {
            _imageFiles = FindImageFiles(directory);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _imageFiles = [];
            lvImages.Items.Clear();
            lblProjectionState.Text = "读取图片目录失败";
            AppendLog($"读取图片目录失败：{ex.Message}");
            if (showError)
            {
                MessageBox.Show(this, ex.Message, "读取图片目录失败",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        _currentImageIndex = -1;
        lvImages.BeginUpdate();
        try
        {
            lvImages.Items.Clear();
            for (int index = 0; index < _imageFiles.Count; index++)
            {
                var item = new ListViewItem((index + 1).ToString());
                item.SubItems.Add(Path.GetFileName(_imageFiles[index]));
                item.SubItems.Add("待投图");
                item.Tag = _imageFiles[index];
                lvImages.Items.Add(item);
            }
        }
        finally
        {
            lvImages.EndUpdate();
        }

        grpImages.Text = $"投影图片（{_imageFiles.Count} 张）";
        lblProjectionState.Text = $"已加载 {_imageFiles.Count} 张图片";
        if (_imageFiles.Count > 0) lvImages.Items[0].Selected = true;

        if (showError && _imageFiles.Count == 0)
        {
            MessageBox.Show(this, "当前目录没有找到 PNG、BMP、JPG 或 JPEG 图片。",
                "图片数量", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        return _imageFiles.Count > 0;
    }

    internal static List<string> FindImageFiles(string directory) =>
        Directory.EnumerateFiles(directory)
            .Where(path => SupportedExtensions.Contains(Path.GetExtension(path)))
            .OrderBy(path => Path.GetFileName(path), StringComparer.CurrentCultureIgnoreCase)
            .ToList();

    internal static IReadOnlyList<int> BuildImageTestOrder(int startIndex, int imageCount)
    {
        if (imageCount <= 0) throw new ArgumentOutOfRangeException(nameof(imageCount));
        if (startIndex < 0 || startIndex >= imageCount)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }

        return Enumerable.Range(0, imageCount)
            .Select(offset => (startIndex + offset) % imageCount)
            .ToArray();
    }

    private void ResetImageTestStatuses()
    {
        foreach (ListViewItem item in lvImages.Items)
        {
            item.SubItems[2].Text = "待测试";
        }
    }

    private void lvImages_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (lvImages.SelectedItems.Count == 1 &&
            lvImages.SelectedItems[0].Tag is string path)
        {
            ShowPreview(path);
        }
    }

    private void ShowPreview(string imagePath)
    {
        try
        {
            using Image source = Image.FromFile(imagePath);
            Size bounds = picPreview.ClientSize;
            double scale = Math.Min(
                (double)Math.Max(1, bounds.Width) / source.Width,
                (double)Math.Max(1, bounds.Height) / source.Height);
            int width = Math.Max(1, (int)Math.Round(source.Width * Math.Min(1d, scale)));
            int height = Math.Max(1, (int)Math.Round(source.Height * Math.Min(1d, scale)));
            Image preview = new Bitmap(source, width, height);
            Image? oldImage = picPreview.Image;
            picPreview.Image = preview;
            oldImage?.Dispose();
        }
        catch
        {
            // 预览失败不阻止正式投图；正式投图会给出错误原因。
        }
    }

    private void CaptureOriginalWallpaper()
    {
        if (_originalWallpaper is null)
        {
            _originalWallpaper = _desktopDisplay.GetCurrentWallpaper();
        }
    }

    private void RestoreOriginalWallpaper(bool showError)
    {
        string? wallpaper = _originalWallpaper;
        _originalWallpaper = null;
        if (string.IsNullOrWhiteSpace(wallpaper) || !File.Exists(wallpaper)) return;

        try
        {
            _desktopDisplay.SetWallpaper(wallpaper);
            AppendLog("已恢复程序启动前的桌面图片。");
        }
        catch (Exception ex)
        {
            AppendLog($"恢复原桌面失败：{ex.Message}");
            if (showError)
            {
                MessageBox.Show(this, ex.Message, "恢复原桌面失败",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void AppendLog(string message) =>
        txtProjectionLog.AppendText(
            $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");

    #endregion

    #region 批量联测

    /// <summary>
    /// 从选中图片开始，每张图发送同一份冻结的指令并等待最终返回。
    /// 断线、主动断开、失败、超时或用户停止都会取消后续投图。
    /// </summary>
    private async void btnBatchTest_Click(object? sender, EventArgs e)
    {
        if (_isBatchRunning)
        {
            btnBatchTest.Enabled = false;
            btnBatchTest.Text = "正在停止...";
            _batchCancellation?.Cancel();
            return;
        }

        if (!EnsureConnected() ||
            !TryGetCurrentMessage(out string frozenMessage, showError: true)) return;

        if (_isTimedProjectionRunning)
        {
            StopTimedProjection("定时投图已停止，开始批量联测。", restoreWallpaper: false);
        }

        if (_imageFiles.Count == 0)
        {
            RefreshImageList(showError: true);
            if (_imageFiles.Count == 0) return;
        }

        int startIndex = lvImages.SelectedIndices.Count == 1
            ? lvImages.SelectedIndices[0]
            : _currentImageIndex >= 0 ? _currentImageIndex : 0;
        IReadOnlyList<int> order = BuildImageTestOrder(startIndex, _imageFiles.Count);

        var batchCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            _formCancellation.Token);
        _batchCancellation = batchCancellation;
        _frozenBatchMessage = frozenMessage;
        SetBatchState(true);
        ResetImageTestStatuses();
        AppendLog(
            $"开始批量联测：共 {order.Count} 张；冻结指令 {frozenMessage}；从 {Path.GetFileName(_imageFiles[startIndex])} 开始。");

        try
        {
            for (int position = 0; position < order.Count; position++)
            {
                CancellationToken token = batchCancellation.Token;
                token.ThrowIfCancellationRequested();

                if (position > 0)
                {
                    AppendLog("等待 1 秒后投放下一张图片。");
                    await Task.Delay(TimeSpan.FromSeconds(1), token);
                }

                token.ThrowIfCancellationRequested();
                if (!_tcpClient.IsConnected)
                {
                    throw new IOException("服务器连接已断开。");
                }

                int imageIndex = order[position];
                if (!ProjectImageAtIndex(
                        imageIndex,
                        showError: true,
                        cancellationToken: token))
                {
                    lvImages.Items[imageIndex].SubItems[2].Text = "投图失败";
                    lblProjectionState.Text = $"联测停止：第 {position + 1} 张投图失败";
                    return;
                }

                ListViewItem item = lvImages.Items[imageIndex];
                item.SubItems[2].Text = "测试中";
                lblProjectionState.Text =
                    $"批量联测 {position + 1}/{order.Count}：{Path.GetFileName(_imageFiles[imageIndex])}";
                AppendLog($"[{position + 1}/{order.Count}] 投图完成，发送 {frozenMessage}");

                bool completed = await SendAndWaitForCompletionAsync(
                    frozenMessage, token);
                token.ThrowIfCancellationRequested();
                if (!completed)
                {
                    item.SubItems[2].Text = "测试失败";
                    lblProjectionState.Text =
                        $"联测停止：第 {position + 1} 张发送或确认失败";
                    return;
                }

                item.SubItems[2].Text = "测试完成";
                AppendLog($"[{position + 1}/{order.Count}] 测试完成。");
            }

            lblProjectionState.Text = $"批量联测完成：{order.Count} 张图片";
            AppendLog($"批量联测全部完成：共 {order.Count} 张图片。");
            MessageBox.Show(this, $"文件夹内 {order.Count} 张图片已全部测试完成。",
                "批量联测完成", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (OperationCanceledException)
        {
            if (!_isClosing)
            {
                lblProjectionState.Text = "批量联测已停止";
                AppendLog("批量联测已停止，不再继续投图。");
            }
        }
        catch (Exception ex)
        {
            if (!_isClosing)
            {
                lblProjectionState.Text = "批量联测已停止";
                AppendLog($"批量联测异常：{ex.Message}");
            }
        }
        finally
        {
            if (ReferenceEquals(_batchCancellation, batchCancellation))
            {
                _batchCancellation = null;
            }
            batchCancellation.Dispose();
            _frozenBatchMessage = null;
            if (!IsDisposed && !Disposing)
            {
                SetBatchState(false);
                UpdateBatchCommandLabel();
            }
        }
    }

    private async Task<bool> SendAndWaitForCompletionAsync(
        string request,
        CancellationToken cancellationToken)
    {
        if (!_tcpClient.IsConnected) return false;

        var completion = new TaskCompletionSource<string>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        void OnMessage(object? sender, string response)
        {
            switch (HudResponseMatcher.Classify(request, response))
            {
                case ResponseClassification.Success:
                    completion.TrySetResult(response);
                    break;
                case ResponseClassification.Failure:
                    completion.TrySetException(new InvalidOperationException(
                        $"光学软件返回失败：{response}"));
                    break;
            }
        }

        void OnClosed(object? sender, ConnectionClosedEventArgs args) =>
            completion.TrySetException(new IOException(args.Description));

        _tcpClient.MessageReceived += OnMessage;
        _tcpClient.ConnectionClosed += OnClosed;
        _isSending = true;
        UpdateActionAvailability();

        using var operation = CancellationTokenSource.CreateLinkedTokenSource(
            _formCancellation.Token, cancellationToken);
        try
        {
            await _tcpClient.SendAsync(request, operation.Token);
            AppendTcpLog("发送", request);

            TimeSpan timeout = HudResponseMatcher.GetTimeout(request);
            AppendLog($"等待光学软件返回，超时 {timeout.TotalSeconds:0} 秒。");
            string response = await completion.Task.WaitAsync(timeout, operation.Token);
            AppendLog($"已确认测试完成：{response}");
            return true;
        }
        catch (TimeoutException)
        {
            AppendLog("等待光学软件返回超时。联测已停止。");
            return false;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (Exception ex)
        {
            AppendLog($"发送或确认失败：{ex.Message}");
            return false;
        }
        finally
        {
            _tcpClient.MessageReceived -= OnMessage;
            _tcpClient.ConnectionClosed -= OnClosed;
            _isSending = false;
            if (!_isClosing) UpdateActionAvailability();
        }
    }

    private void SetBatchState(bool running)
    {
        _isBatchRunning = running;
        grpSettings.Enabled = !running && !_isTimedProjectionRunning;
        btnRefreshImages.Enabled = !running && !_isTimedProjectionRunning;
        btnProjectNext.Enabled = !running && !_isTimedProjectionRunning;
        btnProjectSelected.Enabled = !running && !_isTimedProjectionRunning;
        lvImages.Enabled = !running && !_isTimedProjectionRunning;
        btnTimedProjection.Enabled = !running;
        btnBatchTest.Enabled = true;
        btnBatchTest.Text = running ? "停止批量联测" : "开始批量联测";
        if (running)
        {
            dgvFields.Enabled = false;
        }
        else if (_selectedDefinition is not null)
        {
            TrySynchronizeFieldsFromRaw(
                _selectedDefinition, txtRawMessage.Text, updateGrid: true);
        }
        UpdateActionAvailability();
        UpdateBatchCommandLabel();
    }

    private void UpdateActionAvailability()
    {
        bool connected = _tcpClient.IsConnected;
        btnSendOnce.Enabled = connected && !_isSending && !_isBatchRunning;
        btnBatchTest.Enabled = _isBatchRunning ||
            (connected && !_isTimedProjectionRunning && !_isSending);
    }

    #endregion

    #region 窗体生命周期

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        _isClosing = true;
        _batchCancellation?.Cancel();
        _formCancellation.Cancel();

        if (_isTimedProjectionRunning)
        {
            StopTimedProjection(null, restoreWallpaper: true);
        }
        else if (chkRestoreWallpaper.Checked)
        {
            RestoreOriginalWallpaper(showError: false);
        }
    }

    private async void MainForm_FormClosed(object? sender, FormClosedEventArgs e)
    {
        _tcpClient.Connected -= TcpClient_Connected;
        _tcpClient.MessageReceived -= TcpClient_MessageReceived;
        _tcpClient.ConnectionClosed -= TcpClient_ConnectionClosed;
        try
        {
            await _tcpClient.DisposeAsync();
        }
        catch
        {
            // 窗口已经关闭，释放阶段不再打扰用户。
        }
        DisposeFormCancellation();
    }

    private void PostToUi(Action action)
    {
        if (_isClosing || IsDisposed || Disposing) return;
        try
        {
            if (InvokeRequired) BeginInvoke(action);
            else action();
        }
        catch (InvalidOperationException) when (_isClosing || IsDisposed || Disposing)
        {
            // 句柄销毁期间忽略迟到的网络事件。
        }
    }

    /// <summary>由 Designer.Dispose 调用，保证窗体级取消源只释放一次。</summary>
    private void DisposeFormCancellation()
    {
        if (_formCancellationDisposed) return;

        _formCancellationDisposed = true;
        _formCancellation.Cancel();
        _formCancellation.Dispose();
    }

    #endregion
}
