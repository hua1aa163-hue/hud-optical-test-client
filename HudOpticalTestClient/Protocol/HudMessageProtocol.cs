using System.Globalization;
using System.Text.RegularExpressions;

namespace HudOpticalTestClient.Protocol;

/// <summary>根据可视字段构建 HUD 命令，并校验用户手工编辑的发送内容。</summary>
public static partial class HudMessageProtocol
{
    private const int MaxMessageLength = 64 * 1024;

    /// <summary>按目录代码和字段字典构建命令；输入不合法时抛出 ArgumentException。</summary>
    public static string BuildMessage(
        string code,
        IReadOnlyDictionary<string, string?>? fieldValues = null)
    {
        if (!TryBuildMessage(code, fieldValues, out string message, out string error))
        {
            throw new ArgumentException(error, nameof(fieldValues));
        }

        return message;
    }

    /// <summary>按定义和字段字典构建命令。</summary>
    public static string BuildMessage(
        TestDefinition definition,
        IReadOnlyDictionary<string, string?>? fieldValues = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        return BuildMessage(definition.Code, fieldValues);
    }

    /// <summary>尝试按目录代码和字段字典构建命令。</summary>
    public static bool TryBuildMessage(
        string code,
        IReadOnlyDictionary<string, string?>? fieldValues,
        out string message,
        out string error)
    {
        message = string.Empty;
        error = string.Empty;

        if (!HudCommandCatalog.TryGet(code, out TestDefinition definition))
        {
            error = $"未知协议命令：{code}";
            return false;
        }

        fieldValues ??= EmptyFields.Instance;

        switch (definition.Code)
        {
            case "t11":
                {
                    string x = GetValue(fieldValues, "xTranslation").Trim();
                    string y = GetValue(fieldValues, "yTranslation").Trim();
                    if (x.Length == 0 && y.Length == 0)
                    {
                        message = "t11";
                        return true;
                    }

                    if (x.Length == 0 || y.Length == 0)
                    {
                        error = "t11 的 X 平移距离和 Y 平移距离必须同时填写，或同时留空。";
                        return false;
                    }

                    if (!IsProtocolNumber(x) || !IsProtocolNumber(y))
                    {
                        error = "t11 平移距离必须是使用小数点的有限数字，例如 10、-2.5。";
                        return false;
                    }

                    message = $"t11/{x}/{y}";
                    break;
                }

            case "c-":
                if (!TryGetRequired(fieldValues, "recipe", "配方名称", out string recipe, out error))
                {
                    return false;
                }
                if (!HasNoControlOrTerminator(recipe))
                {
                    error = "配方名称不能包含换行、NUL 或 %。";
                    return false;
                }
                message = $"c-{recipe}";
                break;

            case "gin":
                message = "gin";
                break;

            case "pic-":
                if (!TryGetRequired(fieldValues, "filePath", "完整图片路径", out string filePath, out error))
                {
                    return false;
                }
                if (ContainsLineBreakOrNull(filePath))
                {
                    error = "图片路径不能包含换行或 NUL。";
                    return false;
                }
                message = $"pic-{filePath}";
                break;

            case "bmp-":
                if (!TryGetRequired(fieldValues, "directory", "保存目录", out string directory, out error) ||
                    !TryGetRequired(fieldValues, "baseName", "文件基名", out string baseName, out error))
                {
                    return false;
                }
                if (ContainsLineBreakOrNull(directory) || ContainsLineBreakOrNull(baseName) ||
                    directory.Contains('|') || baseName.Contains('|'))
                {
                    error = "BMP 保存目录和文件基名不能包含换行、NUL 或竖线 |。";
                    return false;
                }
                message = $"bmp-{directory}|{baseName}";
                break;

            case "n-":
                if (!TryGetRequired(fieldValues, "workbookName", "工作簿名", out string workbookName, out error))
                {
                    return false;
                }
                if (ContainsLineBreakOrNull(workbookName) ||
                    workbookName.Contains(',') || workbookName.Contains('%'))
                {
                    error = "工作簿名不能包含换行、NUL、逗号或 %；不需要填写 .xlsx。";
                    return false;
                }
                message = $"n-{workbookName},%";
                break;

            default:
                // t1～t10、t12～t21 请求本身没有参数，发送代码即可。
                message = definition.DefaultMessage;
                break;
        }

        if (!ValidateRaw(message, out string normalized, out error))
        {
            message = string.Empty;
            return false;
        }

        message = normalized;
        return true;
    }

    /// <summary>校验手工编辑内容。</summary>
    public static bool ValidateRaw(string? rawMessage, out string error)
    {
        ProtocolValidationResult result = ValidateRaw(rawMessage);
        error = result.Error;
        return result.Valid;
    }

    /// <summary>校验并返回去除首尾空白、规范化命令前缀后的文本。</summary>
    public static bool ValidateRaw(
        string? rawMessage,
        out string normalizedMessage,
        out string error)
    {
        ProtocolValidationResult result = ValidateRaw(rawMessage);
        normalizedMessage = result.NormalizedMessage;
        error = result.Error;
        return result.Valid;
    }

    /// <summary>返回结构化校验结果。</summary>
    public static ProtocolValidationResult ValidateRaw(string? rawMessage)
    {
        if (string.IsNullOrWhiteSpace(rawMessage))
        {
            return ProtocolValidationResult.Failure("发送内容不能为空。 ");
        }

        // 必须在 Trim 之前检查；否则末尾的 \r/\n 会被当成普通空白吞掉，
        // 手工粘贴的多行命令就可能错误地通过校验。
        if (ContainsLineBreakOrNull(rawMessage))
        {
            return ProtocolValidationResult.Failure("一条命令不能包含回车、换行或 NUL。 ");
        }

        string raw = rawMessage.Trim();
        if (raw.Length > MaxMessageLength)
        {
            return ProtocolValidationResult.Failure("发送内容超过 64 KiB 限制。 ");
        }

        Match testMatch = TestCommandRegex().Match(raw);
        if (testMatch.Success &&
            int.TryParse(testMatch.Groups["number"].Value, out int testNumber) &&
            testNumber is >= 1 and <= 21)
        {
            return ProtocolValidationResult.Success($"t{testNumber}");
        }

        Match t11Match = T11ParameterizedRegex().Match(raw);
        if (t11Match.Success)
        {
            string x = t11Match.Groups["x"].Value.Trim();
            string y = t11Match.Groups["y"].Value.Trim();
            if (!IsProtocolNumber(x) || !IsProtocolNumber(y))
            {
                return ProtocolValidationResult.Failure(
                    "t11 平移距离必须是使用小数点的有限数字，例如 t11/10/20。 ");
            }

            return ProtocolValidationResult.Success($"t11/{x}/{y}");
        }

        if (raw.StartsWith("t11/", StringComparison.OrdinalIgnoreCase))
        {
            return ProtocolValidationResult.Failure(
                "t11 仅支持无参数 t11，或成对参数 t11/<X平移距离>/<Y平移距离>。 ");
        }

        if (raw.StartsWith("c-", StringComparison.OrdinalIgnoreCase))
        {
            string recipe = raw[2..].Trim();
            return recipe.Length == 0 || !HasNoControlOrTerminator(recipe)
                ? ProtocolValidationResult.Failure("c- 后必须填写不含换行、NUL 和 % 的配方名称。 ")
                : ProtocolValidationResult.Success($"c-{recipe}");
        }

        if (raw.Equals("gin", StringComparison.OrdinalIgnoreCase))
        {
            return ProtocolValidationResult.Success("gin");
        }

        if (raw.StartsWith("pic-", StringComparison.OrdinalIgnoreCase))
        {
            string path = raw[4..].Trim();
            return path.Length == 0
                ? ProtocolValidationResult.Failure("pic- 后必须填写完整图片路径。 ")
                : ProtocolValidationResult.Success($"pic-{path}");
        }

        if (raw.StartsWith("bmp-", StringComparison.OrdinalIgnoreCase))
        {
            string body = raw[4..];
            int separator = body.IndexOf('|');
            if (separator <= 0 || separator != body.LastIndexOf('|') ||
                separator == body.Length - 1 ||
                string.IsNullOrWhiteSpace(body[..separator]) ||
                string.IsNullOrWhiteSpace(body[(separator + 1)..]))
            {
                return ProtocolValidationResult.Failure(
                    "BMP 命令格式应为 bmp-<保存目录>|<文件基名>。 ");
            }

            string directory = body[..separator].Trim();
            string baseName = body[(separator + 1)..].Trim();
            return ProtocolValidationResult.Success($"bmp-{directory}|{baseName}");
        }

        if (raw.StartsWith("n-", StringComparison.OrdinalIgnoreCase))
        {
            if (!raw.EndsWith(",%", StringComparison.Ordinal))
            {
                return ProtocolValidationResult.Failure(
                    "保存数据命令必须以“,%”结束，例如 n-asdfg,% 。 ");
            }

            string workbookName = raw[2..^2].Trim();
            if (workbookName.Length == 0 || workbookName.Contains(',') || workbookName.Contains('%'))
            {
                return ProtocolValidationResult.Failure(
                    "n- 后必须填写不含逗号和 % 的工作簿名。 ");
            }

            return ProtocolValidationResult.Success($"n-{workbookName},%");
        }

        return ProtocolValidationResult.Failure(
            "发送内容不符合 HUD V3.1：请选择 t1～t21，或 c-/gin/pic-/bmp-/n- 辅助指令。 ");
    }

    /// <summary>根据一条合法或可识别的请求找到目录定义。</summary>
    public static bool TryGetDefinition(string? rawMessage, out TestDefinition definition)
    {
        ProtocolValidationResult result = ValidateRaw(rawMessage);
        if (!result.Valid)
        {
            definition = null!;
            return false;
        }

        string normalized = result.NormalizedMessage;
        string code = normalized switch
        {
            _ when normalized.StartsWith("t11/", StringComparison.Ordinal) => "t11",
            _ when normalized.StartsWith("c-", StringComparison.Ordinal) => "c-",
            _ when normalized.StartsWith("pic-", StringComparison.Ordinal) => "pic-",
            _ when normalized.StartsWith("bmp-", StringComparison.Ordinal) => "bmp-",
            _ when normalized.StartsWith("n-", StringComparison.Ordinal) => "n-",
            _ => normalized
        };
        return HudCommandCatalog.TryGet(code, out definition);
    }

    private static bool TryGetRequired(
        IReadOnlyDictionary<string, string?> values,
        string key,
        string displayName,
        out string value,
        out string error)
    {
        value = GetValue(values, key).Trim();
        if (value.Length == 0)
        {
            error = $"{displayName}不能为空。";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static string GetValue(IReadOnlyDictionary<string, string?> values, string key)
    {
        if (values.TryGetValue(key, out string? direct))
        {
            return direct ?? string.Empty;
        }

        foreach ((string candidateKey, string? candidateValue) in values)
        {
            if (candidateKey.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return candidateValue ?? string.Empty;
            }
        }

        return string.Empty;
    }

    private static bool IsProtocolNumber(string value)
    {
        return double.TryParse(
                   value,
                   NumberStyles.Float,
                   CultureInfo.InvariantCulture,
                   out double number) &&
               double.IsFinite(number);
    }

    private static bool ContainsLineBreakOrNull(string value) =>
        value.IndexOfAny(['\r', '\n', '\0']) >= 0;

    private static bool HasNoControlOrTerminator(string value) =>
        !ContainsLineBreakOrNull(value) && !value.Contains('%');

    [GeneratedRegex(@"^t(?<number>\d{1,2})$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex TestCommandRegex();

    [GeneratedRegex(
        @"^t11/(?<x>[^/]+)/(?<y>[^/]+)$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex T11ParameterizedRegex();

    private sealed class EmptyFields : IReadOnlyDictionary<string, string?>
    {
        public static EmptyFields Instance { get; } = new();
        public int Count => 0;
        public IEnumerable<string> Keys => [];
        public IEnumerable<string?> Values => [];
        public string? this[string key] => throw new KeyNotFoundException();
        public bool ContainsKey(string key) => false;
        public bool TryGetValue(string key, out string? value)
        {
            value = null;
            return false;
        }
        public IEnumerator<KeyValuePair<string, string?>> GetEnumerator() =>
            Enumerable.Empty<KeyValuePair<string, string?>>().GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
