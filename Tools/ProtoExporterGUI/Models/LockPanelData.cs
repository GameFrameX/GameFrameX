using System;
using System.Collections.Generic;
using GameFrameX.ProtoExport.Persistence;

namespace ProtoExporterGUI.Models
{
    /// <summary>
    /// 子 ID lock 状态面板的观测数据快照。
    /// <para>
    /// 只做观测显示：把 <see cref="MessageIdLockStore.Load"/> 的结果与文件系统信息
    /// （是否存在、最后修改时间）拍平成 UI 可直接绑定的形态，不参与任何导出决策。
    /// </para>
    /// </summary>
    public sealed class LockPanelData
    {
        /// <summary>
        /// lock 文件加载结果状态。
        /// </summary>
        public enum LoadState
        {
            /// <summary>文件不存在。</summary>
            NotFound,

            /// <summary>文件存在且解析成功。</summary>
            Found,

            /// <summary>文件存在但解析失败（schema 不兼容 / JSON 损坏 / IO 异常）。</summary>
            Failed,
        }

        /// <summary>被观测的 lock 文件绝对路径（未设置时为 null 或空白）。</summary>
        public string LockFilePath { get; }

        /// <summary>加载结果状态。</summary>
        public LoadState State { get; }

        /// <summary>文件最后修改时间；文件不存在时为 null。</summary>
        public DateTime? LastWriteTime { get; }

        /// <summary>逐模块观测行；仅 <see cref="LoadState.Found"/> 时非空集合。</summary>
        public IReadOnlyList<LockModuleRow> Modules { get; }

        /// <summary>解析失败时的异常信息；其余状态为 null。</summary>
        public string ErrorMessage { get; }

        private LockPanelData(string lockFilePath, LoadState state, DateTime? lastWriteTime,
            IReadOnlyList<LockModuleRow> modules, string errorMessage)
        {
            LockFilePath = lockFilePath;
            State = state;
            LastWriteTime = lastWriteTime;
            Modules = modules;
            ErrorMessage = errorMessage;
        }

        /// <summary>
        /// 观测一个 lock 文件路径并生成面板快照。不抛异常——任何 IO/解析错误都归入
        /// <see cref="LoadState.Failed"/>，保证面板在文件损坏时显示错误而不是让 GUI 崩溃。
        /// </summary>
        /// <param name="lockFilePath">lock 文件路径；null 或空白视为 <see cref="LoadState.NotFound"/>。</param>
        public static LockPanelData Observe(string lockFilePath)
        {
            if (string.IsNullOrWhiteSpace(lockFilePath))
            {
                return new LockPanelData(lockFilePath, LoadState.NotFound, null, EmptyModules(), null);
            }

            if (!System.IO.File.Exists(lockFilePath))
            {
                return new LockPanelData(lockFilePath, LoadState.NotFound, null, EmptyModules(), null);
            }

            DateTime? lastWrite;
            try
            {
                lastWrite = System.IO.File.GetLastWriteTime(lockFilePath);
            }
            catch (Exception)
            {
                lastWrite = null;
            }

            try
            {
                var lockData = MessageIdLockStore.Load(lockFilePath);
                var rows = new List<LockModuleRow>();
                foreach (var pair in lockData.Modules)
                {
                    rows.Add(new LockModuleRow(pair.Key, pair.Value.ModuleName,
                        pair.Value.Messages.Count, pair.Value.Retired.Count));
                }

                return new LockPanelData(lockFilePath, LoadState.Found, lastWrite, rows.AsReadOnly(), null);
            }
            catch (Exception ex)
            {
                return new LockPanelData(lockFilePath, LoadState.Failed, lastWrite, EmptyModules(), ex.Message);
            }
        }

        private static IReadOnlyList<LockModuleRow> EmptyModules()
        {
            return new List<LockModuleRow>().AsReadOnly();
        }

        /// <summary>
        /// 格式化最后修改时间用于面板显示。文件不存在或不可读时返回 null，
        /// 由调用方决定显示文案（如「—」）。
        /// </summary>
        public string FormatLastWriteTime(string format)
        {
            if (!LastWriteTime.HasValue)
            {
                return null;
            }

            return LastWriteTime.Value.ToString(format);
        }
    }
}
