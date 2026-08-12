using System.Text.RegularExpressions;

namespace HudOpticalTestClient.Protocol;

/// <summary>把以“%”结尾的返回消息与当前请求关联，并给出等待超时。</summary>
public static class HudResponseMatcher
{
    /// <summary>光学测试默认等待时间。V3.1 未定义超时，此值属于客户端运行策略。</summary>
    public static readonly TimeSpan OpticalTestTimeout = TimeSpan.FromSeconds(120);

    /// <summary>辅助指令默认等待时间。V3.1 未定义超时，此值属于客户端运行策略。</summary>
    public static readonly TimeSpan AuxiliaryCommandTimeout = TimeSpan.FromSeconds(10);

    /// <summary>判断 response 是当前 request 的成功、失败，还是无关消息。</summary>
    public static ResponseClassification Classify(string request, string response)
    {
        if (string.IsNullOrWhiteSpace(request) || string.IsNullOrWhiteSpace(response))
        {
            return ResponseClassification.Ignored;
        }

        ProtocolValidationResult requestResult = HudMessageProtocol.ValidateRaw(request);
        if (!requestResult.Valid ||
            !HudMessageProtocol.TryGetDefinition(requestResult.NormalizedMessage, out TestDefinition definition))
        {
            return ResponseClassification.Ignored;
        }

        string normalizedRequest = requestResult.NormalizedMessage;
        string incoming = response.Trim();

        if (definition.Category == HudCommandCatalog.AuxiliaryCategory)
        {
            if (incoming.Equals("OK%", StringComparison.OrdinalIgnoreCase))
            {
                return ResponseClassification.Success;
            }

            if (incoming.Equals("Fail%", StringComparison.OrdinalIgnoreCase))
            {
                return ResponseClassification.Failure;
            }

            return ResponseClassification.Ignored;
        }

        // V3.1 只为 t5 明确定义 Error0/Error1；它们分别表示识别点数偏多/偏少。
        if (definition.Code.Equals("t5", StringComparison.OrdinalIgnoreCase) &&
            (incoming.Equals("Error0%", StringComparison.OrdinalIgnoreCase) ||
             incoming.Equals("Error1%", StringComparison.OrdinalIgnoreCase)))
        {
            return ResponseClassification.Failure;
        }

        // 若服务器在其他测试中复用通用失败终态，停止等待比把失败当无关消息直至超时更安全。
        if (incoming.Equals("Fail%", StringComparison.OrdinalIgnoreCase) ||
            incoming.Equals("NG%", StringComparison.OrdinalIgnoreCase))
        {
            return ResponseClassification.Failure;
        }

        string expectedRequestPrefix = normalizedRequest;
        if (IsWellFormedResult(incoming, expectedRequestPrefix))
        {
            return ResponseClassification.Success;
        }

        if (StartsWithResultPrefix(incoming, expectedRequestPrefix))
        {
            // 已确认是当前测试的返回，却缺少冒号、终止符等必要结构，作为失败终态处理。
            return ResponseClassification.Failure;
        }

        if (definition.Code.Equals("t21", StringComparison.OrdinalIgnoreCase))
        {
            // t21 正文示例明确为 t21_Result；末尾说明却误写 t3_Result。
            // 为兼容已按误写实现的服务器，仅在等待 t21 时接受 t3_Result。
            if (IsWellFormedResult(incoming, "t3"))
            {
                return ResponseClassification.Success;
            }

            if (StartsWithResultPrefix(incoming, "t3"))
            {
                return ResponseClassification.Failure;
            }
        }

        return ResponseClassification.Ignored;
    }

    /// <summary>取得请求的默认等待时间：光学测试120秒，辅助指令10秒。</summary>
    public static TimeSpan GetTimeout(string request)
    {
        return HudMessageProtocol.TryGetDefinition(request, out TestDefinition definition) &&
               definition.Category == HudCommandCatalog.OpticalTestCategory
            ? OpticalTestTimeout
            : AuxiliaryCommandTimeout;
    }

    /// <summary>GetTimeout 的语义化别名，便于其他项目迁移。</summary>
    public static TimeSpan GetResponseTimeout(string request) => GetTimeout(request);

    private static bool IsWellFormedResult(string response, string requestPrefix)
    {
        string pattern =
            $@"^{Regex.Escape(requestPrefix)}_Result(?:\(\d+\))?:.*%$";
        return Regex.IsMatch(
            response,
            pattern,
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);
    }

    private static bool StartsWithResultPrefix(string response, string requestPrefix)
    {
        return response.StartsWith(
            $"{requestPrefix}_Result",
            StringComparison.OrdinalIgnoreCase);
    }
}
