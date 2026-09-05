namespace ProtoExporterGUI.Models
{
    /// <summary>
    /// lock 状态面板中的单模块观测行。对应 <see cref="GameFrameX.ProtoExport.Persistence.MessageIdLock.Modules"/>
    /// 中的一个条目，仅携带展示所需的四个字段。
    /// </summary>
    public sealed class LockModuleRow
    {
        /// <summary>模块号（lock JSON 中 modules 的 key，short 的字符串形式）。</summary>
        public string ModuleKey { get; }

        /// <summary>逻辑模块名（proto package 名）。</summary>
        public string ModuleName { get; }

        /// <summary>当前生效的消息数（messages 计数）。</summary>
        public int MessageCount { get; }

        /// <summary>墓碑数（retired 计数，已删除/改名消息保留的历史号）。</summary>
        public int RetiredCount { get; }

        /// <summary>
        /// 模块 ID 的解析来源（fileName / option），取自最近一次导出日志；无导出记录时为「—」。
        /// 其余四列来自 lock 文件快照，此列来自日志观测，故为渲染期后填充的可写属性。
        /// </summary>
        public string ModuleSource { get; set; } = "—";

        public LockModuleRow(string moduleKey, string moduleName, int messageCount, int retiredCount)
        {
            ModuleKey = moduleKey;
            ModuleName = moduleName;
            MessageCount = messageCount;
            RetiredCount = retiredCount;
        }
    }
}
