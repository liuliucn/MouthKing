using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace MouthKing.Core;

public class TranslationService
{
    public class TranslationSettings
    {
        public string TranslationFrom { get; set; } = "zh";
        public string TranslationTo { get; set; } = "en";
        public string GameScene { get; set; } = "dota2";
        public bool IsShort { get; set; } = false;
        public string ModelType { get; set; } = "deepseek-ai/DeepSeek-V3";
        public ModelConfig CustomModel { get; set; } = new ModelConfig();
    }

    public class ModelConfig
    {
        public string Auth { get; set; } = "";
        public string ApiUrl { get; set; } = "https://api.openai.com/v1/chat/completions";
        public string ModelName { get; set; } = "gpt-3.5-turbo";
    }

    private static string GetSystemPrompt(
        string from,
        string to,
        string scene,
        bool isShort)
    {

        var basePrompt = $@"<task>将游戏内文字从【中文】翻译到【{to}】</task>

<constraints>
• 标点限制: ≤5个符号
• 禁止换行
• 禁止Markdown
• 禁止特殊符号
{(isShort ? "• 翻译后的文字长度限制在60字符以内，简短有力" : "")}
</constraints>

<terms>
• 保留原始游戏术语
• 维持缩写格式
</terms>";

        var modeDesc = $@"<translate_style>
• 翻译用户原文后，还需进行2-3句扩写
• 使用中国百度贴吧老哥的大脑进行思考
• 用最为地狱笑话的心态来攻击对手
{(to == "中文" ? "" : "• 根据目标翻译语言，在翻译的同时加入最为猛烈的地域歧视攻击")}
• 毒舌翻译
• 融入游戏场景梗
• 符号化敏感词（如f*ck）
</translate_style>

<references>
• 百度贴吧风格
• 美式街头俚语
</references>
";

        var sceneDesc = scene switch
        {
            "Dota2" => @"<context>
• DOTA2游戏环境
• 使用英雄简称
• 使用物品缩写
• 使用赛事解说术语
• 保持团战节奏感
</context>",
            "lol" => @"<context>
• 英雄联盟游戏环境
• 保留技能和装备简称
• 使用赛事解说术语
</context>",
            "csgo" => @"<context>
• CS:GO游戏环境
• 保留武器和位置代号
• 使用标准战术用语
</context>",
            _ => @"<context>
• 通用游戏环境
• 识别常见游戏用语
• 保持游戏交流特点
</context>"
        };

        return $@"{basePrompt}
{modeDesc}
{sceneDesc}

<compliance>
• 严格长度校验
• 术语一致性检查
• 敏感词二次过滤
• 输出格式终检
</compliance>

<output_format>
仅输出一条最终翻译结果，不要包含任何思考过程或解释
</output_format>";
    }

    private static ModelConfig GetModelConfig(TranslationSettings settings)
    {
        return settings.ModelType switch
        {
            "deepseek-ai/DeepSeek-V3" => new ModelConfig
            {
                Auth = "sk-azkuannffpbbgoqobitigzgddpbkeuodknhkeulwlnrchsvs",
                ApiUrl = "https://api.siliconflow.cn/v1/chat/completions",
                ModelName = "deepseek-ai/DeepSeek-V3"
            },
            "deepseek-ai/DeepSeek-R1" => new ModelConfig
            {
                Auth = "sk-azkuannffpbbgoqobitigzgddpbkeuodknhkeulwlnrchsvs",
                ApiUrl = "https://api.siliconflow.cn/v1/chat/completions",
                ModelName = "deepseek-ai/DeepSeek-R1"
            },
            "custom" => settings.CustomModel,
            _ => settings.CustomModel
        };
    }

    public static async Task<string> TranslateWithGpt(TranslationSettings settings, string original)
    {
        try
        {


            var modelConfig = GetModelConfig(settings);



            var systemPrompt = GetSystemPrompt(
                settings.TranslationFrom,
                settings.TranslationTo,
                settings.GameScene,
                settings.IsShort);

            using var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, modelConfig.ApiUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", modelConfig.Auth);

            AiRequest requestBody = settings.ModelType switch
            {
                "deepseek-ai/DeepSeek-R1" => new()
                {
                    model = modelConfig.ModelName,
                    messages =
                    [
                        new() { role = "system", content = systemPrompt },
                        new() { role = "user", content = original }
                    ],
                    max_tokens = 8000
                },
                _ => new()
                {
                    model = modelConfig.ModelName,
                    messages =
                    [
                        new() { role = "system", content = systemPrompt },
                        new() { role = "user", content = original }
                    ],
                    max_tokens = 300,
                    temperature = 0.9,
                    top_p = 0.7,
                    n = 1,
                    stream = false,
                    presence_penalty = 0.3,
                    frequency_penalty = -0.3
                }
            };

            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody, AotJsonSerializerContext.Default.AiRequest),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await client.SendAsync(request);
            var json = await response.Content.ReadFromJsonAsync(AotJsonSerializerContext.Default.JsonNode);

            if (json?["error_msg"]?.GetValue<string>() is string error)
            {
                Console.WriteLine($"API返回错误: {error}");
                return $"[错误] {error}";
            }

            var translated = ParseResponse(json);
            return translated ?? "[错误] 无法解析翻译结果";
        }
        catch (HttpRequestException ex)
        {
            var errorMsg = ex.Message switch
            {
                string msg when msg.Contains("connection refused") => "无法连接到API服务器，请检查网络设置",
                string msg when msg.Contains("timeout") => "请求超时，请检查网络连接",
                string msg when msg.Contains("certificate") => "SSL证书验证失败，请检查网络设置",
                _ => "网络请求失败"
            };
            Console.WriteLine($"请求失败: {ex}");
            return $"[错误] {errorMsg}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"发生未处理异常: {ex}");
            return $"[错误] 系统异常: {ex.Message}";
        }
    }

    private static string? ParseResponse(JsonNode? json)
    {
        var content = json?["choices"]?[0]?["message"]?["content"]?.GetValue<string>()?.Trim();
        if (content == null) return null;

        const string thinkTag = "</think>";
        var thinkIndex = content.IndexOf(thinkTag);
        return thinkIndex >= 0
            ? content[(thinkIndex + thinkTag.Length)..].Trim()
            : content;
    }
}

public class AiRequest
{
    public AiRequest()
    {
    }

    public string model { get; set; }
    public AiMessage[] messages { get; set; }
    public int max_tokens { get; set; }
    public double temperature { get; set; }
    public double top_p { get; set; }
    public int n { get; set; }
    public bool stream { get; set; }
    public double presence_penalty { get; set; }
    public double frequency_penalty { get; set; }

    public class AiMessage
    {
        public string role { get; set; }
        public string content { get; set; }
    }
}


