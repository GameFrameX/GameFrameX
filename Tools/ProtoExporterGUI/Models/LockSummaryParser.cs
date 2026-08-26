using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ProtoExporterGUI.Models
{
    /// <summary>
    /// 从导出日志中提取本次 lock 变更统计（模块数 / 新增 SubId 条数）。
    /// </summary>
    /// <remarks>
    /// 导出器（ProtoExport.ProtoBufMessageHandler.Start）完成 lock 分配后会输出一行
    /// <c>[Lock] 涉及模块 2 个，新增 SubId 3 条：…</c> 的日志。GUI 只做观测，
    /// 不重新解析 lock 文件，直接从这行日志读回统计。
    /// 正则按数字捕获，不锚定中文文案细节（冒号后的新增清单长度可变），避免文案微调即失效。
    /// </remarks>
    public static class LockSummaryParser
    {
        /// <summary>匹配导出器输出的 lock 统计日志行。</summary>
        private static readonly Regex SummaryPattern = new Regex(
            @"^\[Lock\]\D*(?<modules>\d+)\D*?(?<newly>\d+)",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// 尝试从一行日志中解析统计。匹配失败（含 null 输入）返回 false，不抛异常。
        /// </summary>
        /// <param name="logLine">单行日志文本。</param>
        /// <param name="moduleCount">解析出的模块数。</param>
        /// <param name="newlyAssignedCount">解析出的新增 SubId 条数。</param>
        public static bool TryParse(string logLine, out int moduleCount, out int newlyAssignedCount)
        {
            moduleCount = 0;
            newlyAssignedCount = 0;
            if (string.IsNullOrEmpty(logLine))
            {
                return false;
            }

            var match = SummaryPattern.Match(logLine);
            if (!match.Success)
            {
                return false;
            }

            if (!int.TryParse(match.Groups["modules"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out moduleCount))
            {
                return false;
            }

            if (!int.TryParse(match.Groups["newly"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out newlyAssignedCount))
            {
                return false;
            }

            return true;
        }
    }
}
