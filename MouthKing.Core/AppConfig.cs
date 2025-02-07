using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MouthKing.Core;
public class AppConfig
{
    public string Hotkey { get; set; } = "Ctrl + T";
    public string ToLanguage { get; set; } = "东南亚英语";
    public string Game { get; set; } = "Dota2";
    public string AiModel { get; set; } = "deepseek-ai/DeepSeek-V3";
    public bool IsShort { get; set; }
    public bool IsNoTranslate { get; set; }
}