using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ProtoExporterGUI.Models
{
    /// <summary>
    /// 从导出日志中提取各模块的 module 来源（文件名前缀 / option 声明）。
    /// </summary>
    /// <remarks>
    /// 导出器（ProtoExport.MessageHelper.Parse）解析每个 proto 文件后输出一行
    /// <c>Package X =&gt; Module 10 (from fileName)</c>（英文）/ <c>包 X =&gt; 模块 10（来源 fileName）</c>（中文）
    /// 的日志，语言随 UI culture 切换。GUI 只做观测，从日志读回 module → source 映射，
    /// 与 lock 面板按模块号关联显示。正则同时兼容中英文两种行格式，来源 token（fileName/option）保持字面。
    /// </remarks>
    public static class ModuleSourceParser
    {
        /// <summary>匹配导出器输出的 module 来源日志行（中英文双语兼容，含历史冒号格式）。</summary>
        private static readonly Regex SourcePattern = new Regex(
            @"=>\s*(?:Module|模块):?\s*(?<module>-?\d+)\s*[(（]\s*(?:from|来源)[:\s]\s*(?<source>fileName|option)\s*[)）]",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// 从多行日志中收集 module → source 映射。同一模块出现多次时以后出现的为准（最后一次导出生效）。
        /// 匹配不到任何行时返回空字典，不抛异常（含 null 行输入）。
        /// </summary>
        public static Dictionary<short, string> Collect(IEnumerable<string> logLines)
        {
            var result = new Dictionary<short, string>();
            if (logLines == null)
            {
                return result;
            }

            foreach (var line in logLines)
            {
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                var match = SourcePattern.Match(line);
                if (!match.Success)
                {
                    continue;
                }

                if (!short.TryParse(match.Groups["module"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var module))
                {
                    continue;
                }

                result[module] = match.Groups["source"].Value;
            }

            return result;
        }
    }
}
