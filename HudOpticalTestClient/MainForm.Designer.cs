#nullable disable

namespace HudOpticalTestClient;

/// <summary>HUD 光学测试客户端主工作台的 WinForms 设计器布局。</summary>
partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    /// <summary>释放取消令牌、预览图片和设计器组件。</summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposeFormCancellation();
            picPreview?.Image?.Dispose();
            components?.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>创建主工作台的固定控件、响应式布局和事件绑定。</summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        projectionTimer = new System.Windows.Forms.Timer(components);
        rootLayout = new TableLayoutPanel();
        pnlConnectionBar = new Panel();
        connectionLayout = new TableLayoutPanel();
        lblAppTitle = new Label();
        lblHostCaption = new Label();
        txtHost = new TextBox();
        lblPortCaption = new Label();
        numPort = new NumericUpDown();
        btnConnect = new Button();
        lblConnectionStatus = new Label();
        mainTabs = new TabControl();
        tabProjection = new TabPage();
        projectionRootLayout = new TableLayoutPanel();
        grpSettings = new GroupBox();
        projectionSettingsLayout = new TableLayoutPanel();
        lblImageDirectory = new Label();
        txtImageDirectory = new TextBox();
        btnBrowseDirectory = new Button();
        lblProjectionState = new Label();
        projectionOptionsFlow = new FlowLayoutPanel();
        lblTopology = new Label();
        cmbTopology = new ComboBox();
        btnApplyTopology = new Button();
        lblProjectionIntervalSeconds = new Label();
        numProjectionIntervalSeconds = new NumericUpDown();
        btnTimedProjection = new Button();
        chkRestoreWallpaper = new CheckBox();
        projectionSplit = new SplitContainer();
        grpImages = new GroupBox();
        lvImages = new ListView();
        colIndex = new ColumnHeader();
        colFileName = new ColumnHeader();
        colStatus = new ColumnHeader();
        previewLayout = new TableLayoutPanel();
        lblPreviewTitle = new Label();
        picPreview = new PictureBox();
        lblCurrentImage = new Label();
        lblProjectionLogTitle = new Label();
        txtProjectionLog = new TextBox();
        projectionActionsFlow = new FlowLayoutPanel();
        btnRefreshImages = new Button();
        btnProjectSelected = new Button();
        btnProjectNext = new Button();
        lblBatchCommand = new Label();
        btnChooseCommand = new Button();
        btnBatchTest = new Button();
        tabProtocol = new TabPage();
        protocolSplit = new SplitContainer();
        commandCatalogLayout = new TableLayoutPanel();
        lblCommandCatalogTitle = new Label();
        lblCommandCatalogHint = new Label();
        dgvCommands = new DataGridView();
        colCommandCategory = new DataGridViewTextBoxColumn();
        colCommandCode = new DataGridViewTextBoxColumn();
        colCommandName = new DataGridViewTextBoxColumn();
        colCommandMessage = new DataGridViewTextBoxColumn();
        commandDetailLayout = new TableLayoutPanel();
        lblSelectedCommandTitle = new Label();
        lblDescriptionCaption = new Label();
        txtCommandDescription = new TextBox();
        lblFieldsCaption = new Label();
        dgvFields = new DataGridView();
        colFieldName = new DataGridViewTextBoxColumn();
        colFieldMeaning = new DataGridViewTextBoxColumn();
        colFieldValue = new DataGridViewTextBoxColumn();
        lblRawMessageCaption = new Label();
        txtRawMessage = new TextBox();
        lblResponseHint = new Label();
        protocolActionsFlow = new FlowLayoutPanel();
        btnRestoreDefault = new Button();
        btnSendOnce = new Button();
        lblResponseFieldsCaption = new Label();
        dgvResponseFields = new DataGridView();
        colResponseIndex = new DataGridViewTextBoxColumn();
        colResponseFieldName = new DataGridViewTextBoxColumn();
        tabCommunication = new TabPage();
        communicationLayout = new TableLayoutPanel();
        pnlCommunicationHeader = new Panel();
        lblCommunicationTitle = new Label();
        lblCommunicationHint = new Label();
        txtTcpLog = new TextBox();
        communicationActionsFlow = new FlowLayoutPanel();
        btnClearTcpLog = new Button();
        rootLayout.SuspendLayout();
        pnlConnectionBar.SuspendLayout();
        connectionLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
        mainTabs.SuspendLayout();
        tabProjection.SuspendLayout();
        projectionRootLayout.SuspendLayout();
        grpSettings.SuspendLayout();
        projectionSettingsLayout.SuspendLayout();
        projectionOptionsFlow.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numProjectionIntervalSeconds).BeginInit();
        ((System.ComponentModel.ISupportInitialize)projectionSplit).BeginInit();
        projectionSplit.Panel1.SuspendLayout();
        projectionSplit.Panel2.SuspendLayout();
        projectionSplit.SuspendLayout();
        grpImages.SuspendLayout();
        previewLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
        projectionActionsFlow.SuspendLayout();
        tabProtocol.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)protocolSplit).BeginInit();
        protocolSplit.Panel1.SuspendLayout();
        protocolSplit.Panel2.SuspendLayout();
        protocolSplit.SuspendLayout();
        commandCatalogLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvCommands).BeginInit();
        commandDetailLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvFields).BeginInit();
        protocolActionsFlow.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvResponseFields).BeginInit();
        tabCommunication.SuspendLayout();
        communicationLayout.SuspendLayout();
        pnlCommunicationHeader.SuspendLayout();
        communicationActionsFlow.SuspendLayout();
        SuspendLayout();
        // 
        // projectionTimer
        // 
        projectionTimer.Interval = 5000;
        projectionTimer.Tick += projectionTimer_Tick;
        // 
        // rootLayout
        // 
        rootLayout.BackColor = Color.FromArgb(242, 246, 250);
        rootLayout.ColumnCount = 1;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.Controls.Add(pnlConnectionBar, 0, 0);
        rootLayout.Controls.Add(mainTabs, 0, 1);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.Location = new Point(0, 0);
        rootLayout.Name = "rootLayout";
        rootLayout.Padding = new Padding(14);
        rootLayout.RowCount = 2;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.Size = new Size(1440, 900);
        rootLayout.TabIndex = 0;
        // 
        // pnlConnectionBar
        // 
        pnlConnectionBar.BackColor = Color.White;
        pnlConnectionBar.BorderStyle = BorderStyle.FixedSingle;
        pnlConnectionBar.Controls.Add(connectionLayout);
        pnlConnectionBar.Dock = DockStyle.Fill;
        pnlConnectionBar.Location = new Point(17, 17);
        pnlConnectionBar.Margin = new Padding(3, 3, 3, 9);
        pnlConnectionBar.Name = "pnlConnectionBar";
        pnlConnectionBar.Padding = new Padding(14, 10, 14, 10);
        pnlConnectionBar.Size = new Size(1406, 64);
        pnlConnectionBar.TabIndex = 0;
        // 
        // connectionLayout
        // 
        connectionLayout.ColumnCount = 7;
        connectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 270F));
        connectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 82F));
        connectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        connectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 62F));
        connectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112F));
        connectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 116F));
        connectionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 196F));
        connectionLayout.Controls.Add(lblAppTitle, 0, 0);
        connectionLayout.Controls.Add(lblHostCaption, 1, 0);
        connectionLayout.Controls.Add(txtHost, 2, 0);
        connectionLayout.Controls.Add(lblPortCaption, 3, 0);
        connectionLayout.Controls.Add(numPort, 4, 0);
        connectionLayout.Controls.Add(btnConnect, 5, 0);
        connectionLayout.Controls.Add(lblConnectionStatus, 6, 0);
        connectionLayout.Dock = DockStyle.Fill;
        connectionLayout.Location = new Point(14, 10);
        connectionLayout.Name = "connectionLayout";
        connectionLayout.RowCount = 1;
        connectionLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        connectionLayout.Size = new Size(1376, 42);
        connectionLayout.TabIndex = 0;
        // 
        // lblAppTitle
        // 
        lblAppTitle.Dock = DockStyle.Fill;
        lblAppTitle.Font = new Font("Microsoft YaHei UI", 13.5F, FontStyle.Bold);
        lblAppTitle.ForeColor = Color.FromArgb(31, 55, 78);
        lblAppTitle.Location = new Point(3, 0);
        lblAppTitle.Name = "lblAppTitle";
        lblAppTitle.Size = new Size(264, 42);
        lblAppTitle.TabIndex = 0;
        lblAppTitle.Text = "HUD 光学测试工作台";
        lblAppTitle.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblHostCaption
        // 
        lblHostCaption.Dock = DockStyle.Fill;
        lblHostCaption.ForeColor = Color.FromArgb(72, 88, 104);
        lblHostCaption.Location = new Point(273, 0);
        lblHostCaption.Name = "lblHostCaption";
        lblHostCaption.Size = new Size(76, 42);
        lblHostCaption.TabIndex = 1;
        lblHostCaption.Text = "服务器：";
        lblHostCaption.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtHost
        // 
        txtHost.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtHost.BorderStyle = BorderStyle.FixedSingle;
        txtHost.Location = new Point(355, 7);
        txtHost.Margin = new Padding(3, 0, 10, 0);
        txtHost.Name = "txtHost";
        txtHost.Size = new Size(675, 28);
        txtHost.TabIndex = 2;
        txtHost.Text = "127.0.0.1";
        // 
        // lblPortCaption
        // 
        lblPortCaption.Dock = DockStyle.Fill;
        lblPortCaption.ForeColor = Color.FromArgb(72, 88, 104);
        lblPortCaption.Location = new Point(1043, 0);
        lblPortCaption.Name = "lblPortCaption";
        lblPortCaption.Size = new Size(56, 42);
        lblPortCaption.TabIndex = 3;
        lblPortCaption.Text = "端口：";
        lblPortCaption.TextAlign = ContentAlignment.MiddleRight;
        // 
        // numPort
        // 
        numPort.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        numPort.BorderStyle = BorderStyle.FixedSingle;
        numPort.Location = new Point(1105, 7);
        numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
        numPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numPort.Name = "numPort";
        numPort.Size = new Size(106, 28);
        numPort.TabIndex = 4;
        numPort.TextAlign = HorizontalAlignment.Center;
        numPort.Value = new decimal(new int[] { 5556, 0, 0, 0 });
        // 
        // btnConnect
        // 
        btnConnect.BackColor = Color.FromArgb(37, 99, 163);
        btnConnect.Dock = DockStyle.Fill;
        btnConnect.FlatAppearance.BorderSize = 0;
        btnConnect.FlatStyle = FlatStyle.Flat;
        btnConnect.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        btnConnect.ForeColor = Color.White;
        btnConnect.Location = new Point(1220, 3);
        btnConnect.Margin = new Padding(6, 3, 6, 3);
        btnConnect.Name = "btnConnect";
        btnConnect.Size = new Size(104, 36);
        btnConnect.TabIndex = 5;
        btnConnect.Text = "连接";
        btnConnect.UseVisualStyleBackColor = false;
        btnConnect.Click += btnConnect_Click;
        // 
        // lblConnectionStatus
        // 
        lblConnectionStatus.BackColor = Color.FromArgb(247, 238, 238);
        lblConnectionStatus.BorderStyle = BorderStyle.FixedSingle;
        lblConnectionStatus.Dock = DockStyle.Fill;
        lblConnectionStatus.ForeColor = Color.FromArgb(145, 54, 54);
        lblConnectionStatus.Location = new Point(1333, 3);
        lblConnectionStatus.Margin = new Padding(3);
        lblConnectionStatus.Name = "lblConnectionStatus";
        lblConnectionStatus.Size = new Size(190, 36);
        lblConnectionStatus.TabIndex = 6;
        lblConnectionStatus.Text = "● 未连接";
        lblConnectionStatus.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // mainTabs
        // 
        mainTabs.Controls.Add(tabProjection);
        mainTabs.Controls.Add(tabProtocol);
        mainTabs.Controls.Add(tabCommunication);
        mainTabs.Dock = DockStyle.Fill;
        mainTabs.Font = new Font("Microsoft YaHei UI", 10F);
        mainTabs.Location = new Point(17, 93);
        mainTabs.Margin = new Padding(3);
        mainTabs.Name = "mainTabs";
        mainTabs.Padding = new Point(24, 8);
        mainTabs.SelectedIndex = 0;
        mainTabs.Size = new Size(1406, 790);
        mainTabs.TabIndex = 1;
        // 
        // tabProjection
        // 
        tabProjection.BackColor = Color.FromArgb(246, 248, 251);
        tabProjection.Controls.Add(projectionRootLayout);
        tabProjection.Location = new Point(4, 39);
        tabProjection.Name = "tabProjection";
        tabProjection.Padding = new Padding(12);
        tabProjection.Size = new Size(1398, 747);
        tabProjection.TabIndex = 0;
        tabProjection.Text = "投影与批量测试";
        // 
        // projectionRootLayout
        // 
        projectionRootLayout.ColumnCount = 1;
        projectionRootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        projectionRootLayout.Controls.Add(grpSettings, 0, 0);
        projectionRootLayout.Controls.Add(projectionSplit, 0, 1);
        projectionRootLayout.Controls.Add(projectionActionsFlow, 0, 2);
        projectionRootLayout.Dock = DockStyle.Fill;
        projectionRootLayout.Location = new Point(12, 12);
        projectionRootLayout.Name = "projectionRootLayout";
        projectionRootLayout.RowCount = 3;
        projectionRootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 132F));
        projectionRootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        projectionRootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62F));
        projectionRootLayout.Size = new Size(1374, 723);
        projectionRootLayout.TabIndex = 0;
        // 
        // grpSettings
        // 
        grpSettings.BackColor = Color.White;
        grpSettings.Controls.Add(projectionSettingsLayout);
        grpSettings.Dock = DockStyle.Fill;
        grpSettings.ForeColor = Color.FromArgb(39, 59, 78);
        grpSettings.Location = new Point(3, 3);
        grpSettings.Margin = new Padding(3, 3, 3, 8);
        grpSettings.Name = "grpSettings";
        grpSettings.Padding = new Padding(14, 10, 14, 10);
        grpSettings.Size = new Size(1368, 121);
        grpSettings.TabIndex = 0;
        grpSettings.TabStop = false;
        grpSettings.Text = "投影设置";
        // 
        // projectionSettingsLayout
        // 
        projectionSettingsLayout.ColumnCount = 4;
        projectionSettingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
        projectionSettingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        projectionSettingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 132F));
        projectionSettingsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220F));
        projectionSettingsLayout.Controls.Add(lblImageDirectory, 0, 0);
        projectionSettingsLayout.Controls.Add(txtImageDirectory, 1, 0);
        projectionSettingsLayout.Controls.Add(btnBrowseDirectory, 2, 0);
        projectionSettingsLayout.Controls.Add(lblProjectionState, 3, 0);
        projectionSettingsLayout.Controls.Add(projectionOptionsFlow, 0, 1);
        projectionSettingsLayout.Dock = DockStyle.Fill;
        projectionSettingsLayout.Location = new Point(14, 31);
        projectionSettingsLayout.Name = "projectionSettingsLayout";
        projectionSettingsLayout.RowCount = 2;
        projectionSettingsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        projectionSettingsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        projectionSettingsLayout.Size = new Size(1340, 80);
        projectionSettingsLayout.TabIndex = 0;
        projectionSettingsLayout.SetColumnSpan(projectionOptionsFlow, 4);
        // 
        // lblImageDirectory
        // 
        lblImageDirectory.Dock = DockStyle.Fill;
        lblImageDirectory.Location = new Point(3, 0);
        lblImageDirectory.Name = "lblImageDirectory";
        lblImageDirectory.Size = new Size(90, 40);
        lblImageDirectory.TabIndex = 0;
        lblImageDirectory.Text = "图片目录：";
        lblImageDirectory.TextAlign = ContentAlignment.MiddleRight;
        // 
        // txtImageDirectory
        // 
        txtImageDirectory.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        txtImageDirectory.BorderStyle = BorderStyle.FixedSingle;
        txtImageDirectory.Location = new Point(99, 6);
        txtImageDirectory.Name = "txtImageDirectory";
        txtImageDirectory.Size = new Size(883, 28);
        txtImageDirectory.TabIndex = 1;
        // 
        // btnBrowseDirectory
        // 
        btnBrowseDirectory.BackColor = Color.FromArgb(235, 241, 247);
        btnBrowseDirectory.Dock = DockStyle.Fill;
        btnBrowseDirectory.FlatAppearance.BorderColor = Color.FromArgb(183, 198, 213);
        btnBrowseDirectory.FlatStyle = FlatStyle.Flat;
        btnBrowseDirectory.Location = new Point(992, 3);
        btnBrowseDirectory.Margin = new Padding(7, 3, 7, 3);
        btnBrowseDirectory.Name = "btnBrowseDirectory";
        btnBrowseDirectory.Size = new Size(118, 34);
        btnBrowseDirectory.TabIndex = 2;
        btnBrowseDirectory.Text = "选择目录...";
        btnBrowseDirectory.UseVisualStyleBackColor = false;
        btnBrowseDirectory.Click += btnBrowseDirectory_Click;
        // 
        // lblProjectionState
        // 
        lblProjectionState.AutoEllipsis = true;
        lblProjectionState.BackColor = Color.FromArgb(239, 244, 249);
        lblProjectionState.BorderStyle = BorderStyle.FixedSingle;
        lblProjectionState.Dock = DockStyle.Fill;
        lblProjectionState.ForeColor = Color.FromArgb(74, 91, 108);
        lblProjectionState.Location = new Point(1123, 3);
        lblProjectionState.Margin = new Padding(6, 3, 3, 3);
        lblProjectionState.Name = "lblProjectionState";
        lblProjectionState.Size = new Size(214, 34);
        lblProjectionState.TabIndex = 3;
        lblProjectionState.Text = "未启动定时投图";
        lblProjectionState.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // projectionOptionsFlow
        // 
        projectionOptionsFlow.AutoScroll = true;
        projectionOptionsFlow.Controls.Add(lblTopology);
        projectionOptionsFlow.Controls.Add(cmbTopology);
        projectionOptionsFlow.Controls.Add(btnApplyTopology);
        projectionOptionsFlow.Controls.Add(lblProjectionIntervalSeconds);
        projectionOptionsFlow.Controls.Add(numProjectionIntervalSeconds);
        projectionOptionsFlow.Controls.Add(btnTimedProjection);
        projectionOptionsFlow.Controls.Add(chkRestoreWallpaper);
        projectionOptionsFlow.Dock = DockStyle.Fill;
        projectionOptionsFlow.Location = new Point(0, 40);
        projectionOptionsFlow.Margin = new Padding(0);
        projectionOptionsFlow.Name = "projectionOptionsFlow";
        projectionOptionsFlow.Padding = new Padding(0, 5, 0, 0);
        projectionOptionsFlow.Size = new Size(1340, 40);
        projectionOptionsFlow.TabIndex = 4;
        projectionOptionsFlow.WrapContents = false;
        // 
        // lblTopology
        // 
        lblTopology.Location = new Point(3, 5);
        lblTopology.Margin = new Padding(3, 0, 0, 0);
        lblTopology.Name = "lblTopology";
        lblTopology.Size = new Size(90, 32);
        lblTopology.TabIndex = 0;
        lblTopology.Text = "投影模式：";
        lblTopology.TextAlign = ContentAlignment.MiddleRight;
        // 
        // cmbTopology
        // 
        cmbTopology.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbTopology.FormattingEnabled = true;
        cmbTopology.Items.AddRange(new object[] { "不切换投影模式", "仅电脑屏幕", "复制屏幕", "仅第二屏幕", "扩展屏幕" });
        cmbTopology.Location = new Point(99, 7);
        cmbTopology.Margin = new Padding(6, 2, 8, 0);
        cmbTopology.Name = "cmbTopology";
        cmbTopology.Size = new Size(166, 28);
        cmbTopology.TabIndex = 1;
        // 
        // btnApplyTopology
        // 
        btnApplyTopology.BackColor = Color.FromArgb(235, 241, 247);
        btnApplyTopology.FlatAppearance.BorderColor = Color.FromArgb(183, 198, 213);
        btnApplyTopology.FlatStyle = FlatStyle.Flat;
        btnApplyTopology.Location = new Point(276, 7);
        btnApplyTopology.Margin = new Padding(3, 2, 14, 0);
        btnApplyTopology.Name = "btnApplyTopology";
        btnApplyTopology.Size = new Size(162, 32);
        btnApplyTopology.TabIndex = 2;
        btnApplyTopology.Text = "立即应用投影模式";
        btnApplyTopology.UseVisualStyleBackColor = false;
        btnApplyTopology.Click += btnApplyTopology_Click;
        // 
        // lblProjectionIntervalSeconds
        // 
        lblProjectionIntervalSeconds.Location = new Point(455, 5);
        lblProjectionIntervalSeconds.Margin = new Padding(3, 0, 0, 0);
        lblProjectionIntervalSeconds.Name = "lblProjectionIntervalSeconds";
        lblProjectionIntervalSeconds.Size = new Size(118, 32);
        lblProjectionIntervalSeconds.TabIndex = 3;
        lblProjectionIntervalSeconds.Text = "投图间隔(秒)：";
        lblProjectionIntervalSeconds.TextAlign = ContentAlignment.MiddleRight;
        // 
        // numProjectionIntervalSeconds
        // 
        numProjectionIntervalSeconds.Location = new Point(579, 7);
        numProjectionIntervalSeconds.Margin = new Padding(6, 2, 12, 0);
        numProjectionIntervalSeconds.Maximum = new decimal(new int[] { 3600, 0, 0, 0 });
        numProjectionIntervalSeconds.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        numProjectionIntervalSeconds.Name = "numProjectionIntervalSeconds";
        numProjectionIntervalSeconds.Size = new Size(82, 28);
        numProjectionIntervalSeconds.TabIndex = 4;
        numProjectionIntervalSeconds.TextAlign = HorizontalAlignment.Center;
        numProjectionIntervalSeconds.Value = new decimal(new int[] { 5, 0, 0, 0 });
        // 
        // btnTimedProjection
        // 
        btnTimedProjection.BackColor = Color.FromArgb(229, 238, 247);
        btnTimedProjection.FlatAppearance.BorderColor = Color.FromArgb(159, 181, 204);
        btnTimedProjection.FlatStyle = FlatStyle.Flat;
        btnTimedProjection.Location = new Point(676, 7);
        btnTimedProjection.Margin = new Padding(3, 2, 14, 0);
        btnTimedProjection.Name = "btnTimedProjection";
        btnTimedProjection.Size = new Size(150, 32);
        btnTimedProjection.TabIndex = 5;
        btnTimedProjection.Text = "开始定时投图";
        btnTimedProjection.UseVisualStyleBackColor = false;
        btnTimedProjection.Click += btnTimedProjection_Click;
        // 
        // chkRestoreWallpaper
        // 
        chkRestoreWallpaper.AutoSize = true;
        chkRestoreWallpaper.Checked = true;
        chkRestoreWallpaper.CheckState = CheckState.Checked;
        chkRestoreWallpaper.Location = new Point(843, 11);
        chkRestoreWallpaper.Margin = new Padding(3, 6, 3, 0);
        chkRestoreWallpaper.Name = "chkRestoreWallpaper";
        chkRestoreWallpaper.Size = new Size(189, 24);
        chkRestoreWallpaper.TabIndex = 6;
        chkRestoreWallpaper.Text = "停止/关闭后恢复桌面";
        chkRestoreWallpaper.UseVisualStyleBackColor = true;
        // 
        // projectionSplit
        // 
        projectionSplit.BackColor = Color.FromArgb(216, 224, 232);
        projectionSplit.Dock = DockStyle.Fill;
        projectionSplit.Location = new Point(3, 135);
        projectionSplit.Name = "projectionSplit";
        // 
        // projectionSplit.Panel1
        // 
        projectionSplit.Panel1.BackColor = Color.White;
        projectionSplit.Panel1.Controls.Add(grpImages);
        projectionSplit.Panel1MinSize = 520;
        // 
        // projectionSplit.Panel2
        // 
        projectionSplit.Panel2.BackColor = Color.White;
        projectionSplit.Panel2.Controls.Add(previewLayout);
        projectionSplit.Panel2MinSize = 380;
        projectionSplit.Size = new Size(1368, 523);
        projectionSplit.SplitterDistance = 774;
        projectionSplit.SplitterWidth = 8;
        projectionSplit.TabIndex = 1;
        // 
        // grpImages
        // 
        grpImages.BackColor = Color.White;
        grpImages.Controls.Add(lvImages);
        grpImages.Dock = DockStyle.Fill;
        grpImages.ForeColor = Color.FromArgb(39, 59, 78);
        grpImages.Location = new Point(0, 0);
        grpImages.Name = "grpImages";
        grpImages.Padding = new Padding(10, 8, 10, 10);
        grpImages.Size = new Size(774, 523);
        grpImages.TabIndex = 0;
        grpImages.TabStop = false;
        grpImages.Text = "图片列表";
        // 
        // lvImages
        // 
        lvImages.BackColor = Color.White;
        lvImages.BorderStyle = BorderStyle.FixedSingle;
        lvImages.Columns.AddRange(new ColumnHeader[] { colIndex, colFileName, colStatus });
        lvImages.Dock = DockStyle.Fill;
        lvImages.FullRowSelect = true;
        lvImages.GridLines = true;
        lvImages.HideSelection = false;
        lvImages.Location = new Point(10, 29);
        lvImages.MultiSelect = false;
        lvImages.Name = "lvImages";
        lvImages.Size = new Size(754, 484);
        lvImages.TabIndex = 0;
        lvImages.UseCompatibleStateImageBehavior = false;
        lvImages.View = View.Details;
        lvImages.SelectedIndexChanged += lvImages_SelectedIndexChanged;
        // 
        // colIndex
        // 
        colIndex.Text = "序号";
        colIndex.Width = 68;
        // 
        // colFileName
        // 
        colFileName.Text = "文件名";
        colFileName.Width = 500;
        // 
        // colStatus
        // 
        colStatus.Text = "状态";
        colStatus.Width = 130;
        // 
        // previewLayout
        // 
        previewLayout.ColumnCount = 1;
        previewLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        previewLayout.Controls.Add(lblPreviewTitle, 0, 0);
        previewLayout.Controls.Add(picPreview, 0, 1);
        previewLayout.Controls.Add(lblCurrentImage, 0, 2);
        previewLayout.Controls.Add(lblProjectionLogTitle, 0, 3);
        previewLayout.Controls.Add(txtProjectionLog, 0, 4);
        previewLayout.Dock = DockStyle.Fill;
        previewLayout.Location = new Point(0, 0);
        previewLayout.Name = "previewLayout";
        previewLayout.Padding = new Padding(12, 8, 12, 10);
        previewLayout.RowCount = 5;
        previewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        previewLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 62F));
        previewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        previewLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        previewLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 38F));
        previewLayout.Size = new Size(586, 523);
        previewLayout.TabIndex = 0;
        // 
        // lblPreviewTitle
        // 
        lblPreviewTitle.Dock = DockStyle.Fill;
        lblPreviewTitle.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        lblPreviewTitle.ForeColor = Color.FromArgb(39, 59, 78);
        lblPreviewTitle.Location = new Point(15, 8);
        lblPreviewTitle.Name = "lblPreviewTitle";
        lblPreviewTitle.Size = new Size(556, 30);
        lblPreviewTitle.TabIndex = 0;
        lblPreviewTitle.Text = "投影预览";
        lblPreviewTitle.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // picPreview
        // 
        picPreview.BackColor = Color.FromArgb(25, 32, 40);
        picPreview.BorderStyle = BorderStyle.FixedSingle;
        picPreview.Dock = DockStyle.Fill;
        picPreview.Location = new Point(15, 41);
        picPreview.Name = "picPreview";
        picPreview.Size = new Size(556, 252);
        picPreview.SizeMode = PictureBoxSizeMode.Zoom;
        picPreview.TabIndex = 1;
        picPreview.TabStop = false;
        // 
        // lblCurrentImage
        // 
        lblCurrentImage.AutoEllipsis = true;
        lblCurrentImage.Dock = DockStyle.Fill;
        lblCurrentImage.ForeColor = Color.FromArgb(65, 81, 97);
        lblCurrentImage.Location = new Point(15, 296);
        lblCurrentImage.Name = "lblCurrentImage";
        lblCurrentImage.Size = new Size(556, 34);
        lblCurrentImage.TabIndex = 2;
        lblCurrentImage.Text = "当前图片：无";
        lblCurrentImage.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblProjectionLogTitle
        // 
        lblProjectionLogTitle.Dock = DockStyle.Fill;
        lblProjectionLogTitle.Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold);
        lblProjectionLogTitle.ForeColor = Color.FromArgb(39, 59, 78);
        lblProjectionLogTitle.Location = new Point(15, 330);
        lblProjectionLogTitle.Name = "lblProjectionLogTitle";
        lblProjectionLogTitle.Size = new Size(556, 28);
        lblProjectionLogTitle.TabIndex = 3;
        lblProjectionLogTitle.Text = "投影日志";
        lblProjectionLogTitle.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtProjectionLog
        // 
        txtProjectionLog.BackColor = Color.FromArgb(250, 252, 254);
        txtProjectionLog.BorderStyle = BorderStyle.FixedSingle;
        txtProjectionLog.Dock = DockStyle.Fill;
        txtProjectionLog.Font = new Font("Consolas", 9F);
        txtProjectionLog.Location = new Point(15, 361);
        txtProjectionLog.Multiline = true;
        txtProjectionLog.Name = "txtProjectionLog";
        txtProjectionLog.ReadOnly = true;
        txtProjectionLog.ScrollBars = ScrollBars.Vertical;
        txtProjectionLog.Size = new Size(556, 149);
        txtProjectionLog.TabIndex = 4;
        // 
        // projectionActionsFlow
        // 
        projectionActionsFlow.AutoScroll = true;
        projectionActionsFlow.BackColor = Color.FromArgb(246, 248, 251);
        projectionActionsFlow.Controls.Add(btnRefreshImages);
        projectionActionsFlow.Controls.Add(btnProjectSelected);
        projectionActionsFlow.Controls.Add(btnProjectNext);
        projectionActionsFlow.Controls.Add(lblBatchCommand);
        projectionActionsFlow.Controls.Add(btnChooseCommand);
        projectionActionsFlow.Controls.Add(btnBatchTest);
        projectionActionsFlow.Dock = DockStyle.Fill;
        projectionActionsFlow.Location = new Point(0, 661);
        projectionActionsFlow.Margin = new Padding(0);
        projectionActionsFlow.Name = "projectionActionsFlow";
        projectionActionsFlow.Padding = new Padding(0, 10, 0, 4);
        projectionActionsFlow.Size = new Size(1374, 62);
        projectionActionsFlow.TabIndex = 2;
        projectionActionsFlow.WrapContents = false;
        // 
        // btnRefreshImages
        // 
        btnRefreshImages.BackColor = Color.FromArgb(235, 241, 247);
        btnRefreshImages.FlatAppearance.BorderColor = Color.FromArgb(183, 198, 213);
        btnRefreshImages.FlatStyle = FlatStyle.Flat;
        btnRefreshImages.Location = new Point(3, 13);
        btnRefreshImages.Name = "btnRefreshImages";
        btnRefreshImages.Size = new Size(138, 40);
        btnRefreshImages.TabIndex = 0;
        btnRefreshImages.Text = "刷新图片列表";
        btnRefreshImages.UseVisualStyleBackColor = false;
        btnRefreshImages.Click += btnRefreshImages_Click;
        // 
        // btnProjectSelected
        // 
        btnProjectSelected.BackColor = Color.FromArgb(231, 241, 250);
        btnProjectSelected.FlatAppearance.BorderColor = Color.FromArgb(164, 188, 211);
        btnProjectSelected.FlatStyle = FlatStyle.Flat;
        btnProjectSelected.Location = new Point(147, 13);
        btnProjectSelected.Name = "btnProjectSelected";
        btnProjectSelected.Size = new Size(146, 40);
        btnProjectSelected.TabIndex = 1;
        btnProjectSelected.Text = "投放选中图片";
        btnProjectSelected.UseVisualStyleBackColor = false;
        btnProjectSelected.Click += btnProjectSelected_Click;
        // 
        // btnProjectNext
        // 
        btnProjectNext.BackColor = Color.FromArgb(231, 241, 250);
        btnProjectNext.FlatAppearance.BorderColor = Color.FromArgb(164, 188, 211);
        btnProjectNext.FlatStyle = FlatStyle.Flat;
        btnProjectNext.Location = new Point(299, 13);
        btnProjectNext.Margin = new Padding(3, 3, 12, 3);
        btnProjectNext.Name = "btnProjectNext";
        btnProjectNext.Size = new Size(152, 40);
        btnProjectNext.TabIndex = 2;
        btnProjectNext.Text = "切换并投下一张";
        btnProjectNext.UseVisualStyleBackColor = false;
        btnProjectNext.Click += btnProjectNext_Click;
        // 
        // lblBatchCommand
        // 
        lblBatchCommand.AutoEllipsis = true;
        lblBatchCommand.BackColor = Color.White;
        lblBatchCommand.BorderStyle = BorderStyle.FixedSingle;
        lblBatchCommand.ForeColor = Color.FromArgb(64, 80, 96);
        lblBatchCommand.Location = new Point(466, 13);
        lblBatchCommand.Margin = new Padding(3, 3, 6, 3);
        lblBatchCommand.Name = "lblBatchCommand";
        lblBatchCommand.Size = new Size(315, 40);
        lblBatchCommand.TabIndex = 3;
        lblBatchCommand.Text = "批量命令：尚未选择";
        lblBatchCommand.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // btnChooseCommand
        // 
        btnChooseCommand.BackColor = Color.FromArgb(235, 241, 247);
        btnChooseCommand.FlatAppearance.BorderColor = Color.FromArgb(183, 198, 213);
        btnChooseCommand.FlatStyle = FlatStyle.Flat;
        btnChooseCommand.Location = new Point(790, 13);
        btnChooseCommand.Name = "btnChooseCommand";
        btnChooseCommand.Size = new Size(138, 40);
        btnChooseCommand.TabIndex = 4;
        btnChooseCommand.Text = "选择测试命令";
        btnChooseCommand.UseVisualStyleBackColor = false;
        btnChooseCommand.Click += btnChooseCommand_Click;
        // 
        // btnBatchTest
        // 
        btnBatchTest.BackColor = Color.FromArgb(39, 112, 171);
        btnBatchTest.FlatAppearance.BorderSize = 0;
        btnBatchTest.FlatStyle = FlatStyle.Flat;
        btnBatchTest.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        btnBatchTest.ForeColor = Color.White;
        btnBatchTest.Location = new Point(934, 13);
        btnBatchTest.Name = "btnBatchTest";
        btnBatchTest.Size = new Size(152, 40);
        btnBatchTest.TabIndex = 5;
        btnBatchTest.Text = "开始批量测试";
        btnBatchTest.UseVisualStyleBackColor = false;
        btnBatchTest.Click += btnBatchTest_Click;
        // 
        // tabProtocol
        // 
        tabProtocol.BackColor = Color.FromArgb(246, 248, 251);
        tabProtocol.Controls.Add(protocolSplit);
        tabProtocol.Location = new Point(4, 39);
        tabProtocol.Name = "tabProtocol";
        tabProtocol.Padding = new Padding(12);
        tabProtocol.Size = new Size(1398, 747);
        tabProtocol.TabIndex = 1;
        tabProtocol.Text = "协议命令编辑";
        // 
        // protocolSplit
        // 
        protocolSplit.BackColor = Color.FromArgb(216, 224, 232);
        protocolSplit.Dock = DockStyle.Fill;
        protocolSplit.Location = new Point(12, 12);
        protocolSplit.Name = "protocolSplit";
        // 
        // protocolSplit.Panel1
        // 
        protocolSplit.Panel1.BackColor = Color.White;
        protocolSplit.Panel1.Controls.Add(commandCatalogLayout);
        protocolSplit.Panel1MinSize = 520;
        // 
        // protocolSplit.Panel2
        // 
        protocolSplit.Panel2.BackColor = Color.White;
        protocolSplit.Panel2.Controls.Add(commandDetailLayout);
        protocolSplit.Panel2MinSize = 470;
        protocolSplit.Size = new Size(1374, 723);
        protocolSplit.SplitterDistance = 742;
        protocolSplit.SplitterWidth = 8;
        protocolSplit.TabIndex = 0;
        // 
        // commandCatalogLayout
        // 
        commandCatalogLayout.ColumnCount = 1;
        commandCatalogLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        commandCatalogLayout.Controls.Add(lblCommandCatalogTitle, 0, 0);
        commandCatalogLayout.Controls.Add(lblCommandCatalogHint, 0, 1);
        commandCatalogLayout.Controls.Add(dgvCommands, 0, 2);
        commandCatalogLayout.Dock = DockStyle.Fill;
        commandCatalogLayout.Location = new Point(0, 0);
        commandCatalogLayout.Name = "commandCatalogLayout";
        commandCatalogLayout.Padding = new Padding(14, 12, 14, 14);
        commandCatalogLayout.RowCount = 3;
        commandCatalogLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        commandCatalogLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        commandCatalogLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        commandCatalogLayout.Size = new Size(742, 723);
        commandCatalogLayout.TabIndex = 0;
        // 
        // lblCommandCatalogTitle
        // 
        lblCommandCatalogTitle.Dock = DockStyle.Fill;
        lblCommandCatalogTitle.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
        lblCommandCatalogTitle.ForeColor = Color.FromArgb(31, 55, 78);
        lblCommandCatalogTitle.Location = new Point(17, 12);
        lblCommandCatalogTitle.Name = "lblCommandCatalogTitle";
        lblCommandCatalogTitle.Size = new Size(708, 36);
        lblCommandCatalogTitle.TabIndex = 0;
        lblCommandCatalogTitle.Text = "测试命令目录";
        lblCommandCatalogTitle.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblCommandCatalogHint
        // 
        lblCommandCatalogHint.Dock = DockStyle.Fill;
        lblCommandCatalogHint.ForeColor = Color.FromArgb(100, 113, 126);
        lblCommandCatalogHint.Location = new Point(17, 48);
        lblCommandCatalogHint.Name = "lblCommandCatalogHint";
        lblCommandCatalogHint.Size = new Size(708, 34);
        lblCommandCatalogHint.TabIndex = 1;
        lblCommandCatalogHint.Text = "选择命令查看字段含义；“发送内容”列可直接编辑。";
        lblCommandCatalogHint.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // dgvCommands
        // 
        dgvCommands.AllowUserToAddRows = false;
        dgvCommands.AllowUserToDeleteRows = false;
        dgvCommands.AllowUserToResizeRows = false;
        dgvCommands.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        dgvCommands.BackgroundColor = Color.White;
        dgvCommands.BorderStyle = BorderStyle.Fixed3D;
        dgvCommands.ColumnHeadersHeight = 38;
        dgvCommands.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgvCommands.Columns.AddRange(colCommandCategory, colCommandCode, colCommandName, colCommandMessage);
        dgvCommands.Dock = DockStyle.Fill;
        dgvCommands.EditMode = DataGridViewEditMode.EditOnEnter;
        dgvCommands.Location = new Point(17, 85);
        dgvCommands.MultiSelect = false;
        dgvCommands.Name = "dgvCommands";
        dgvCommands.RowHeadersVisible = false;
        dgvCommands.RowTemplate.Height = 34;
        dgvCommands.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvCommands.Size = new Size(708, 621);
        dgvCommands.TabIndex = 2;
        dgvCommands.CellEndEdit += dgvCommands_CellEndEdit;
        dgvCommands.SelectionChanged += dgvCommands_SelectionChanged;
        // 
        // colCommandCategory
        // 
        colCommandCategory.HeaderText = "分类";
        colCommandCategory.MinimumWidth = 90;
        colCommandCategory.Name = "colCommandCategory";
        colCommandCategory.ReadOnly = true;
        colCommandCategory.Width = 105;
        // 
        // colCommandCode
        // 
        colCommandCode.HeaderText = "命令字";
        colCommandCode.MinimumWidth = 90;
        colCommandCode.Name = "colCommandCode";
        colCommandCode.ReadOnly = true;
        colCommandCode.Width = 100;
        // 
        // colCommandName
        // 
        colCommandName.HeaderText = "命令名称";
        colCommandName.MinimumWidth = 130;
        colCommandName.Name = "colCommandName";
        colCommandName.ReadOnly = true;
        colCommandName.Width = 170;
        // 
        // colCommandMessage
        // 
        colCommandMessage.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        colCommandMessage.HeaderText = "发送内容（可编辑）";
        colCommandMessage.MinimumWidth = 240;
        colCommandMessage.Name = "colCommandMessage";
        colCommandMessage.SortMode = DataGridViewColumnSortMode.NotSortable;
        colCommandMessage.DefaultCellStyle.Font = new Font("Consolas", 9F);
        colCommandMessage.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        // 
        // commandDetailLayout
        // 
        commandDetailLayout.ColumnCount = 1;
        commandDetailLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        commandDetailLayout.Controls.Add(lblSelectedCommandTitle, 0, 0);
        commandDetailLayout.Controls.Add(lblDescriptionCaption, 0, 1);
        commandDetailLayout.Controls.Add(txtCommandDescription, 0, 2);
        commandDetailLayout.Controls.Add(lblFieldsCaption, 0, 3);
        commandDetailLayout.Controls.Add(dgvFields, 0, 4);
        commandDetailLayout.Controls.Add(lblRawMessageCaption, 0, 5);
        commandDetailLayout.Controls.Add(txtRawMessage, 0, 6);
        commandDetailLayout.Controls.Add(lblResponseHint, 0, 7);
        commandDetailLayout.Controls.Add(protocolActionsFlow, 0, 8);
        commandDetailLayout.Controls.Add(lblResponseFieldsCaption, 0, 9);
        commandDetailLayout.Controls.Add(dgvResponseFields, 0, 10);
        commandDetailLayout.Dock = DockStyle.Fill;
        commandDetailLayout.Location = new Point(0, 0);
        commandDetailLayout.Name = "commandDetailLayout";
        commandDetailLayout.Padding = new Padding(14, 12, 14, 14);
        commandDetailLayout.RowCount = 11;
        commandDetailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        commandDetailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 26F));
        commandDetailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
        commandDetailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 27F));
        commandDetailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
        commandDetailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 27F));
        commandDetailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66F));
        commandDetailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44F));
        commandDetailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
        commandDetailLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 27F));
        commandDetailLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
        commandDetailLayout.Size = new Size(624, 723);
        commandDetailLayout.TabIndex = 0;
        // 
        // lblSelectedCommandTitle
        // 
        lblSelectedCommandTitle.AutoEllipsis = true;
        lblSelectedCommandTitle.Dock = DockStyle.Fill;
        lblSelectedCommandTitle.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
        lblSelectedCommandTitle.ForeColor = Color.FromArgb(31, 55, 78);
        lblSelectedCommandTitle.Location = new Point(17, 12);
        lblSelectedCommandTitle.Name = "lblSelectedCommandTitle";
        lblSelectedCommandTitle.Size = new Size(590, 42);
        lblSelectedCommandTitle.TabIndex = 0;
        lblSelectedCommandTitle.Text = "请选择测试命令";
        lblSelectedCommandTitle.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblDescriptionCaption
        // 
        lblDescriptionCaption.Dock = DockStyle.Fill;
        lblDescriptionCaption.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        lblDescriptionCaption.ForeColor = Color.FromArgb(67, 83, 99);
        lblDescriptionCaption.Location = new Point(17, 54);
        lblDescriptionCaption.Name = "lblDescriptionCaption";
        lblDescriptionCaption.Size = new Size(590, 26);
        lblDescriptionCaption.TabIndex = 1;
        lblDescriptionCaption.Text = "命令说明";
        lblDescriptionCaption.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtCommandDescription
        // 
        txtCommandDescription.BackColor = Color.FromArgb(248, 250, 252);
        txtCommandDescription.BorderStyle = BorderStyle.FixedSingle;
        txtCommandDescription.Dock = DockStyle.Fill;
        txtCommandDescription.Location = new Point(17, 83);
        txtCommandDescription.Multiline = true;
        txtCommandDescription.Name = "txtCommandDescription";
        txtCommandDescription.ReadOnly = true;
        txtCommandDescription.ScrollBars = ScrollBars.Vertical;
        txtCommandDescription.Size = new Size(590, 66);
        txtCommandDescription.TabIndex = 2;
        // 
        // lblFieldsCaption
        // 
        lblFieldsCaption.Dock = DockStyle.Fill;
        lblFieldsCaption.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        lblFieldsCaption.ForeColor = Color.FromArgb(67, 83, 99);
        lblFieldsCaption.Location = new Point(17, 152);
        lblFieldsCaption.Name = "lblFieldsCaption";
        lblFieldsCaption.Size = new Size(590, 27);
        lblFieldsCaption.TabIndex = 3;
        lblFieldsCaption.Text = "发送字段（仅“值”列可编辑）";
        lblFieldsCaption.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // dgvFields
        // 
        dgvFields.AllowUserToAddRows = false;
        dgvFields.AllowUserToDeleteRows = false;
        dgvFields.AllowUserToResizeRows = false;
        dgvFields.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        dgvFields.BackgroundColor = Color.White;
        dgvFields.ColumnHeadersHeight = 34;
        dgvFields.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgvFields.Columns.AddRange(colFieldName, colFieldMeaning, colFieldValue);
        dgvFields.Dock = DockStyle.Fill;
        dgvFields.EditMode = DataGridViewEditMode.EditOnEnter;
        dgvFields.Location = new Point(17, 182);
        dgvFields.MultiSelect = false;
        dgvFields.Name = "dgvFields";
        dgvFields.RowHeadersVisible = false;
        dgvFields.RowTemplate.Height = 31;
        dgvFields.SelectionMode = DataGridViewSelectionMode.CellSelect;
        dgvFields.Size = new Size(590, 151);
        dgvFields.TabIndex = 4;
        dgvFields.CellEndEdit += dgvFields_CellEndEdit;
        // 
        // colFieldName
        // 
        colFieldName.HeaderText = "字段";
        colFieldName.MinimumWidth = 110;
        colFieldName.Name = "colFieldName";
        colFieldName.ReadOnly = true;
        colFieldName.Width = 135;
        // 
        // colFieldMeaning
        // 
        colFieldMeaning.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        colFieldMeaning.HeaderText = "含义 / 取值说明";
        colFieldMeaning.MinimumWidth = 200;
        colFieldMeaning.Name = "colFieldMeaning";
        colFieldMeaning.ReadOnly = true;
        colFieldMeaning.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        // 
        // colFieldValue
        // 
        colFieldValue.HeaderText = "值（可编辑）";
        colFieldValue.MinimumWidth = 130;
        colFieldValue.Name = "colFieldValue";
        colFieldValue.SortMode = DataGridViewColumnSortMode.NotSortable;
        colFieldValue.Width = 155;
        colFieldValue.DefaultCellStyle.Font = new Font("Consolas", 9F);
        // 
        // lblRawMessageCaption
        // 
        lblRawMessageCaption.Dock = DockStyle.Fill;
        lblRawMessageCaption.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        lblRawMessageCaption.ForeColor = Color.FromArgb(67, 83, 99);
        lblRawMessageCaption.Location = new Point(17, 336);
        lblRawMessageCaption.Name = "lblRawMessageCaption";
        lblRawMessageCaption.Size = new Size(590, 27);
        lblRawMessageCaption.TabIndex = 5;
        lblRawMessageCaption.Text = "完整发送内容（可直接编辑）";
        lblRawMessageCaption.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtRawMessage
        // 
        txtRawMessage.BackColor = Color.FromArgb(252, 253, 255);
        txtRawMessage.BorderStyle = BorderStyle.FixedSingle;
        txtRawMessage.Dock = DockStyle.Fill;
        txtRawMessage.Font = new Font("Consolas", 10F);
        txtRawMessage.Location = new Point(17, 366);
        txtRawMessage.Multiline = true;
        txtRawMessage.Name = "txtRawMessage";
        txtRawMessage.ScrollBars = ScrollBars.Vertical;
        txtRawMessage.Size = new Size(590, 60);
        txtRawMessage.TabIndex = 6;
        txtRawMessage.TextChanged += txtRawMessage_TextChanged;
        // 
        // lblResponseHint
        // 
        lblResponseHint.AutoEllipsis = true;
        lblResponseHint.BackColor = Color.FromArgb(239, 246, 252);
        lblResponseHint.BorderStyle = BorderStyle.FixedSingle;
        lblResponseHint.Dock = DockStyle.Fill;
        lblResponseHint.ForeColor = Color.FromArgb(47, 86, 119);
        lblResponseHint.Location = new Point(17, 429);
        lblResponseHint.Name = "lblResponseHint";
        lblResponseHint.Padding = new Padding(8, 0, 8, 0);
        lblResponseHint.Size = new Size(590, 44);
        lblResponseHint.TabIndex = 7;
        lblResponseHint.Text = "预期返回：选择命令后显示返回含义";
        lblResponseHint.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // protocolActionsFlow
        // 
        protocolActionsFlow.Controls.Add(btnRestoreDefault);
        protocolActionsFlow.Controls.Add(btnSendOnce);
        protocolActionsFlow.Dock = DockStyle.Fill;
        protocolActionsFlow.FlowDirection = FlowDirection.RightToLeft;
        protocolActionsFlow.Location = new Point(14, 473);
        protocolActionsFlow.Margin = new Padding(0);
        protocolActionsFlow.Name = "protocolActionsFlow";
        protocolActionsFlow.Padding = new Padding(0, 5, 0, 3);
        protocolActionsFlow.Size = new Size(596, 50);
        protocolActionsFlow.TabIndex = 8;
        protocolActionsFlow.WrapContents = false;
        // 
        // btnRestoreDefault
        // 
        btnRestoreDefault.BackColor = Color.FromArgb(235, 241, 247);
        btnRestoreDefault.FlatAppearance.BorderColor = Color.FromArgb(183, 198, 213);
        btnRestoreDefault.FlatStyle = FlatStyle.Flat;
        btnRestoreDefault.Location = new Point(443, 8);
        btnRestoreDefault.Name = "btnRestoreDefault";
        btnRestoreDefault.Size = new Size(150, 38);
        btnRestoreDefault.TabIndex = 1;
        btnRestoreDefault.Text = "恢复命令默认值";
        btnRestoreDefault.UseVisualStyleBackColor = false;
        btnRestoreDefault.Click += btnRestoreDefault_Click;
        // 
        // btnSendOnce
        // 
        btnSendOnce.BackColor = Color.FromArgb(37, 99, 163);
        btnSendOnce.FlatAppearance.BorderSize = 0;
        btnSendOnce.FlatStyle = FlatStyle.Flat;
        btnSendOnce.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        btnSendOnce.ForeColor = Color.White;
        btnSendOnce.Location = new Point(287, 8);
        btnSendOnce.Name = "btnSendOnce";
        btnSendOnce.Size = new Size(150, 38);
        btnSendOnce.TabIndex = 0;
        btnSendOnce.Text = "发送当前命令";
        btnSendOnce.UseVisualStyleBackColor = false;
        btnSendOnce.Click += btnSendOnce_Click;
        // 
        // lblResponseFieldsCaption
        // 
        lblResponseFieldsCaption.Dock = DockStyle.Fill;
        lblResponseFieldsCaption.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Bold);
        lblResponseFieldsCaption.ForeColor = Color.FromArgb(67, 83, 99);
        lblResponseFieldsCaption.Location = new Point(17, 523);
        lblResponseFieldsCaption.Name = "lblResponseFieldsCaption";
        lblResponseFieldsCaption.Size = new Size(590, 27);
        lblResponseFieldsCaption.TabIndex = 9;
        lblResponseFieldsCaption.Text = "返回报文字段对照";
        lblResponseFieldsCaption.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // dgvResponseFields
        // 
        dgvResponseFields.AllowUserToAddRows = false;
        dgvResponseFields.AllowUserToDeleteRows = false;
        dgvResponseFields.AllowUserToResizeRows = false;
        dgvResponseFields.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        dgvResponseFields.BackgroundColor = Color.White;
        dgvResponseFields.ColumnHeadersHeight = 34;
        dgvResponseFields.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dgvResponseFields.Columns.AddRange(colResponseIndex, colResponseFieldName);
        dgvResponseFields.Dock = DockStyle.Fill;
        dgvResponseFields.Location = new Point(17, 553);
        dgvResponseFields.MultiSelect = false;
        dgvResponseFields.Name = "dgvResponseFields";
        dgvResponseFields.ReadOnly = true;
        dgvResponseFields.RowHeadersVisible = false;
        dgvResponseFields.RowTemplate.Height = 30;
        dgvResponseFields.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvResponseFields.Size = new Size(590, 153);
        dgvResponseFields.TabIndex = 10;
        // 
        // colResponseIndex
        // 
        colResponseIndex.HeaderText = "序号";
        colResponseIndex.MinimumWidth = 70;
        colResponseIndex.Name = "colResponseIndex";
        colResponseIndex.ReadOnly = true;
        colResponseIndex.Width = 80;
        // 
        // colResponseFieldName
        // 
        colResponseFieldName.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        colResponseFieldName.HeaderText = "字段名称 / 含义";
        colResponseFieldName.MinimumWidth = 240;
        colResponseFieldName.Name = "colResponseFieldName";
        colResponseFieldName.ReadOnly = true;
        colResponseFieldName.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
        // 
        // tabCommunication
        // 
        tabCommunication.BackColor = Color.FromArgb(246, 248, 251);
        tabCommunication.Controls.Add(communicationLayout);
        tabCommunication.Location = new Point(4, 39);
        tabCommunication.Name = "tabCommunication";
        tabCommunication.Padding = new Padding(12);
        tabCommunication.Size = new Size(1398, 747);
        tabCommunication.TabIndex = 2;
        tabCommunication.Text = "通信监视";
        // 
        // communicationLayout
        // 
        communicationLayout.ColumnCount = 1;
        communicationLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        communicationLayout.Controls.Add(pnlCommunicationHeader, 0, 0);
        communicationLayout.Controls.Add(txtTcpLog, 0, 1);
        communicationLayout.Controls.Add(communicationActionsFlow, 0, 2);
        communicationLayout.Dock = DockStyle.Fill;
        communicationLayout.Location = new Point(12, 12);
        communicationLayout.Name = "communicationLayout";
        communicationLayout.RowCount = 3;
        communicationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
        communicationLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        communicationLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        communicationLayout.Size = new Size(1374, 723);
        communicationLayout.TabIndex = 0;
        // 
        // pnlCommunicationHeader
        // 
        pnlCommunicationHeader.BackColor = Color.White;
        pnlCommunicationHeader.BorderStyle = BorderStyle.FixedSingle;
        pnlCommunicationHeader.Controls.Add(lblCommunicationHint);
        pnlCommunicationHeader.Controls.Add(lblCommunicationTitle);
        pnlCommunicationHeader.Dock = DockStyle.Fill;
        pnlCommunicationHeader.Location = new Point(3, 3);
        pnlCommunicationHeader.Margin = new Padding(3, 3, 3, 8);
        pnlCommunicationHeader.Name = "pnlCommunicationHeader";
        pnlCommunicationHeader.Size = new Size(1368, 61);
        pnlCommunicationHeader.TabIndex = 0;
        // 
        // lblCommunicationTitle
        // 
        lblCommunicationTitle.AutoSize = true;
        lblCommunicationTitle.Font = new Font("Microsoft YaHei UI", 11F, FontStyle.Bold);
        lblCommunicationTitle.ForeColor = Color.FromArgb(31, 55, 78);
        lblCommunicationTitle.Location = new Point(17, 9);
        lblCommunicationTitle.Name = "lblCommunicationTitle";
        lblCommunicationTitle.Size = new Size(118, 24);
        lblCommunicationTitle.TabIndex = 0;
        lblCommunicationTitle.Text = "TCP 通信日志";
        // 
        // lblCommunicationHint
        // 
        lblCommunicationHint.AutoSize = true;
        lblCommunicationHint.ForeColor = Color.FromArgb(100, 113, 126);
        lblCommunicationHint.Location = new Point(18, 34);
        lblCommunicationHint.Name = "lblCommunicationHint";
        lblCommunicationHint.Size = new Size(312, 20);
        lblCommunicationHint.TabIndex = 1;
        lblCommunicationHint.Text = "按时间记录连接、发送、接收与异常，便于现场追溯。";
        // 
        // txtTcpLog
        // 
        txtTcpLog.BackColor = Color.FromArgb(20, 28, 36);
        txtTcpLog.BorderStyle = BorderStyle.FixedSingle;
        txtTcpLog.Dock = DockStyle.Fill;
        txtTcpLog.Font = new Font("Consolas", 10F);
        txtTcpLog.ForeColor = Color.FromArgb(217, 228, 237);
        txtTcpLog.Location = new Point(3, 75);
        txtTcpLog.Multiline = true;
        txtTcpLog.Name = "txtTcpLog";
        txtTcpLog.ReadOnly = true;
        txtTcpLog.ScrollBars = ScrollBars.Both;
        txtTcpLog.Size = new Size(1368, 587);
        txtTcpLog.TabIndex = 1;
        txtTcpLog.WordWrap = false;
        // 
        // communicationActionsFlow
        // 
        communicationActionsFlow.Controls.Add(btnClearTcpLog);
        communicationActionsFlow.Dock = DockStyle.Fill;
        communicationActionsFlow.FlowDirection = FlowDirection.RightToLeft;
        communicationActionsFlow.Location = new Point(0, 665);
        communicationActionsFlow.Margin = new Padding(0);
        communicationActionsFlow.Name = "communicationActionsFlow";
        communicationActionsFlow.Padding = new Padding(0, 9, 0, 3);
        communicationActionsFlow.Size = new Size(1374, 58);
        communicationActionsFlow.TabIndex = 2;
        // 
        // btnClearTcpLog
        // 
        btnClearTcpLog.BackColor = Color.FromArgb(235, 241, 247);
        btnClearTcpLog.FlatAppearance.BorderColor = Color.FromArgb(183, 198, 213);
        btnClearTcpLog.FlatStyle = FlatStyle.Flat;
        btnClearTcpLog.Location = new Point(1213, 12);
        btnClearTcpLog.Name = "btnClearTcpLog";
        btnClearTcpLog.Size = new Size(158, 40);
        btnClearTcpLog.TabIndex = 0;
        btnClearTcpLog.Text = "清空通信日志";
        btnClearTcpLog.UseVisualStyleBackColor = false;
        btnClearTcpLog.Click += btnClearTcpLog_Click;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        BackColor = Color.FromArgb(242, 246, 250);
        ClientSize = new Size(1440, 900);
        Controls.Add(rootLayout);
        Font = new Font("Microsoft YaHei UI", 9F);
        MinimumSize = new Size(1280, 820);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "HUD 光学测试客户端";
        FormClosing += MainForm_FormClosing;
        FormClosed += MainForm_FormClosed;
        Load += MainForm_Load;
        rootLayout.ResumeLayout(false);
        pnlConnectionBar.ResumeLayout(false);
        connectionLayout.ResumeLayout(false);
        connectionLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
        mainTabs.ResumeLayout(false);
        tabProjection.ResumeLayout(false);
        projectionRootLayout.ResumeLayout(false);
        grpSettings.ResumeLayout(false);
        projectionSettingsLayout.ResumeLayout(false);
        projectionSettingsLayout.PerformLayout();
        projectionOptionsFlow.ResumeLayout(false);
        projectionOptionsFlow.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)numProjectionIntervalSeconds).EndInit();
        projectionSplit.Panel1.ResumeLayout(false);
        projectionSplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)projectionSplit).EndInit();
        projectionSplit.ResumeLayout(false);
        grpImages.ResumeLayout(false);
        previewLayout.ResumeLayout(false);
        previewLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
        projectionActionsFlow.ResumeLayout(false);
        tabProtocol.ResumeLayout(false);
        protocolSplit.Panel1.ResumeLayout(false);
        protocolSplit.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)protocolSplit).EndInit();
        protocolSplit.ResumeLayout(false);
        commandCatalogLayout.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvCommands).EndInit();
        commandDetailLayout.ResumeLayout(false);
        commandDetailLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvFields).EndInit();
        protocolActionsFlow.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvResponseFields).EndInit();
        tabCommunication.ResumeLayout(false);
        communicationLayout.ResumeLayout(false);
        communicationLayout.PerformLayout();
        pnlCommunicationHeader.ResumeLayout(false);
        pnlCommunicationHeader.PerformLayout();
        communicationActionsFlow.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.Timer projectionTimer;
    private TableLayoutPanel rootLayout;
    private Panel pnlConnectionBar;
    private TableLayoutPanel connectionLayout;
    private Label lblAppTitle;
    private Label lblHostCaption;
    private TextBox txtHost;
    private Label lblPortCaption;
    private NumericUpDown numPort;
    private Button btnConnect;
    private Label lblConnectionStatus;
    private TabControl mainTabs;
    private TabPage tabProjection;
    private TabPage tabProtocol;
    private TabPage tabCommunication;
    private TableLayoutPanel projectionRootLayout;
    private GroupBox grpSettings;
    private TableLayoutPanel projectionSettingsLayout;
    private Label lblImageDirectory;
    private TextBox txtImageDirectory;
    private Button btnBrowseDirectory;
    private Label lblProjectionState;
    private FlowLayoutPanel projectionOptionsFlow;
    private Label lblTopology;
    private ComboBox cmbTopology;
    private Button btnApplyTopology;
    private Label lblProjectionIntervalSeconds;
    private NumericUpDown numProjectionIntervalSeconds;
    private Button btnTimedProjection;
    private CheckBox chkRestoreWallpaper;
    private SplitContainer projectionSplit;
    private GroupBox grpImages;
    private ListView lvImages;
    private ColumnHeader colIndex;
    private ColumnHeader colFileName;
    private ColumnHeader colStatus;
    private TableLayoutPanel previewLayout;
    private Label lblPreviewTitle;
    private PictureBox picPreview;
    private Label lblCurrentImage;
    private Label lblProjectionLogTitle;
    private TextBox txtProjectionLog;
    private FlowLayoutPanel projectionActionsFlow;
    private Button btnRefreshImages;
    private Button btnProjectSelected;
    private Button btnProjectNext;
    private Label lblBatchCommand;
    private Button btnChooseCommand;
    private Button btnBatchTest;
    private SplitContainer protocolSplit;
    private TableLayoutPanel commandCatalogLayout;
    private Label lblCommandCatalogTitle;
    private Label lblCommandCatalogHint;
    private DataGridView dgvCommands;
    private DataGridViewTextBoxColumn colCommandCategory;
    private DataGridViewTextBoxColumn colCommandCode;
    private DataGridViewTextBoxColumn colCommandName;
    private DataGridViewTextBoxColumn colCommandMessage;
    private TableLayoutPanel commandDetailLayout;
    private Label lblSelectedCommandTitle;
    private Label lblDescriptionCaption;
    private TextBox txtCommandDescription;
    private Label lblFieldsCaption;
    private DataGridView dgvFields;
    private DataGridViewTextBoxColumn colFieldName;
    private DataGridViewTextBoxColumn colFieldMeaning;
    private DataGridViewTextBoxColumn colFieldValue;
    private Label lblRawMessageCaption;
    private TextBox txtRawMessage;
    private Label lblResponseHint;
    private FlowLayoutPanel protocolActionsFlow;
    private Button btnRestoreDefault;
    private Button btnSendOnce;
    private Label lblResponseFieldsCaption;
    private DataGridView dgvResponseFields;
    private DataGridViewTextBoxColumn colResponseIndex;
    private DataGridViewTextBoxColumn colResponseFieldName;
    private TableLayoutPanel communicationLayout;
    private Panel pnlCommunicationHeader;
    private Label lblCommunicationTitle;
    private Label lblCommunicationHint;
    private TextBox txtTcpLog;
    private FlowLayoutPanel communicationActionsFlow;
    private Button btnClearTcpLog;
}
