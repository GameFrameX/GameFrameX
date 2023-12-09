using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEditor;
using Debug = UnityEngine.Debug;

namespace Luban.Editor
{
    internal static class GenUtils
    {
        internal static readonly string _DOTNET = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dotnet.exe" : "dotnet";

        /// <summary>
        /// 生成执行
        /// </summary>
        /// <param name="arguments">参数</param>
        /// <param name="before">前置执行器</param>
        /// <param name="after">后置执行器</param>
        public static void Gen(string arguments, string before, string after)
        {
            Debug.Log(arguments);

            IBeforeGen beforeGen = null;

            if (!string.IsNullOrEmpty(before))
            {
                var type = Type.GetType(before);

                if (type != null)
                {
                    beforeGen = Activator.CreateInstance(type) as IBeforeGen;
                }
            }

            IAfterGen afterGen = null;

            if (!string.IsNullOrEmpty(after))
            {
                var type = Type.GetType(after);

                if (type != null)
                {
                    afterGen = Activator.CreateInstance(type) as IAfterGen;
                }
            }

            beforeGen?.Process();

            var process = _Run(
                _DOTNET,
                arguments,
                ".",
                true
            );

            #region 捕捉生成错误

            string processLog = process.StandardOutput.ReadToEnd();
            Debug.Log(processLog);
            if (process.ExitCode != 0)
            {
                Debug.LogError("Error  生成出现错误");
            }

            #endregion

            afterGen?.Process();

            AssetDatabase.Refresh();
        }

        private static Process _Run(string exe, string arguments, string workingDir = ".", bool waitExit = false)
        {
            try
            {
                bool redirectStandardOutput = true;
                bool redirectStandardError = true;
                bool useShellExecute = false;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    redirectStandardOutput = false;
                    redirectStandardError = false;
                    useShellExecute = true;
                }

                if (waitExit)
                {
                    redirectStandardOutput = true;
                    redirectStandardError = true;
                    useShellExecute = false;
                }

                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = useShellExecute,
                    WorkingDirectory = workingDir,
                    RedirectStandardOutput = redirectStandardOutput,
                    RedirectStandardError = redirectStandardError,
                };

                Process process = Process.Start(info);

                if (waitExit)
                {
                    WaitForExitAsync(process).ConfigureAwait(false);
                }

                return process;
            }
            catch (Exception e)
            {
                throw new Exception($"dir: {Path.GetFullPath(workingDir)}, command: {exe} {arguments}", e);
            }
        }

        private static async Task WaitForExitAsync(this Process self)
        {
            if (!self.HasExited)
            {
                return;
            }

            try
            {
                self.EnableRaisingEvents = true;
            }
            catch (InvalidOperationException)
            {
                if (self.HasExited)
                {
                    return;
                }

                throw;
            }

            var tcs = new TaskCompletionSource<bool>();

            void Handler(object s, EventArgs e) => tcs.TrySetResult(true);

            self.Exited += Handler;

            try
            {
                if (self.HasExited)
                {
                    return;
                }

                await tcs.Task;
            }
            finally
            {
                self.Exited -= Handler;
            }
        }
    }
}