namespace HudOpticalTestClient.Protocol;

/// <summary>
/// 一项可显示在字段编辑表格中的协议字段。
/// 请求字段用于构建命令，返回字段用于解释服务器返回值。
/// </summary>
public sealed class ProtocolFieldDefinition
{
    public ProtocolFieldDefinition(
        string key,
        string name,
        string description,
        string defaultValue = "",
        bool required = false,
        string unit = "",
        string example = "",
        bool isRepeated = false)
    {
        Key = string.IsNullOrWhiteSpace(key)
            ? throw new ArgumentException("字段键不能为空。", nameof(key))
            : key;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("字段名称不能为空。", nameof(name))
            : name;
        Description = description ?? string.Empty;
        DefaultValue = defaultValue ?? string.Empty;
        Required = required;
        Unit = unit ?? string.Empty;
        Example = example ?? string.Empty;
        IsRepeated = isRepeated;
    }

    /// <summary>稳定的程序字段键，用于传给 BuildMessage 的字典。</summary>
    public string Key { get; }

    /// <summary>面向用户显示的中文名称。</summary>
    public string Name { get; }

    /// <summary>字段含义、顺序或特殊规则。</summary>
    public string Description { get; }

    /// <summary>新建编辑行时使用的默认值。</summary>
    public string DefaultValue { get; }

    /// <summary>是否必须填写。</summary>
    public bool Required { get; }

    /// <summary>协议明确给出的单位；未明确时为空，避免臆造。</summary>
    public string Unit { get; }

    /// <summary>协议示例或推荐输入形式。</summary>
    public string Example { get; }

    /// <summary>该字段是否位于可变数量的重复组中。</summary>
    public bool IsRepeated { get; }
}

/// <summary>一条 HUD 测试或辅助指令的完整目录定义。</summary>
public sealed class TestDefinition
{
    public TestDefinition(
        string category,
        string code,
        string name,
        string description,
        string defaultMessage,
        string responseHint,
        IEnumerable<ProtocolFieldDefinition>? fields = null,
        IEnumerable<ProtocolFieldDefinition>? responseFields = null)
    {
        Category = string.IsNullOrWhiteSpace(category)
            ? throw new ArgumentException("分类不能为空。", nameof(category))
            : category;
        Code = string.IsNullOrWhiteSpace(code)
            ? throw new ArgumentException("命令代码不能为空。", nameof(code))
            : code;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("命令名称不能为空。", nameof(name))
            : name;
        Description = description ?? string.Empty;
        DefaultMessage = defaultMessage ?? string.Empty;
        ResponseHint = responseHint ?? string.Empty;
        Fields = Array.AsReadOnly((fields ?? []).ToArray());
        ResponseFields = Array.AsReadOnly((responseFields ?? []).ToArray());
    }

    /// <summary>“光学测试”或“辅助指令”。</summary>
    public string Category { get; }

    /// <summary>目录中的稳定代码，例如 t11、c-、bmp-。</summary>
    public string Code { get; }

    /// <summary>中文测试名称。</summary>
    public string Name { get; }

    /// <summary>测试目的、格式说明和已知协议矛盾。</summary>
    public string Description { get; }

    /// <summary>不修改字段时应发送的默认文本。</summary>
    public string DefaultMessage { get; }

    /// <summary>便于操作员对照的返回格式摘要。</summary>
    public string ResponseHint { get; }

    /// <summary>可编辑并参与构建请求的动态字段。</summary>
    public IReadOnlyList<ProtocolFieldDefinition> Fields { get; }

    /// <summary>按协议顺序列出的返回字段含义。</summary>
    public IReadOnlyList<ProtocolFieldDefinition> ResponseFields { get; }

    public override string ToString() => $"{Code} - {Name}";
}

/// <summary>ValidateRaw 的结构化结果。</summary>
public readonly record struct ProtocolValidationResult(
    bool Valid,
    string NormalizedMessage,
    string Error)
{
    public static ProtocolValidationResult Success(string normalizedMessage) =>
        new(true, normalizedMessage, string.Empty);

    public static ProtocolValidationResult Failure(string error) =>
        new(false, string.Empty, error);
}

/// <summary>当前返回消息与等待中的请求之间的关系。</summary>
public enum ResponseClassification
{
    /// <summary>与当前请求无关，或只是尚未定义为终态的消息。</summary>
    Ignored,

    /// <summary>当前请求已经得到成功终态。</summary>
    Success,

    /// <summary>当前请求已经得到失败终态或同类畸形返回。</summary>
    Failure
}
