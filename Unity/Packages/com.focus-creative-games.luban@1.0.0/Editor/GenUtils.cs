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
        internal static readonly string _DOTNET =
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "dotnet.exe" : "dotnet";

        public static void Gen(string arguments, string before, string after)
        {
            Debug.Log(arguments);

            IBeforeGen before_gen = null;

            if (!string.IsNullOrEmpty(before))
            {
                var type = Type.GetType(before);

                if (type != null)
                {
                    before_gen = Activator.CreateInstance(type) as IBeforeGen;
                }
            }

            IAfterGen after_gen = null;

            if (!string.IsNullOrEmpty(after))
            {
                var type = Type.GetType(after);

                if (type != null)
                {
                    after_gen = Activator.CreateInstance(type) as IAfterGen;
                }
            }

            before_gen?.Process();

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

            after_gen?.Process();

            AssetDatabase.Refresh();
        }

        private static Process _Run(string exe,
                                    string arguments,
                                    string workingDir = ".",
                                    bool waitExit = false)
        {
            try
            {
                bool redirect_standard_output = true;
                bool redirect_standard_error = true;
                bool use_shell_execute = false;

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    redirect_standard_output = false;
                    redirect_standard_error = false;
                    use_shell_execute = true;
                }

                if (waitExit)
                {
                    redirect_standard_output = true;
                    redirect_standard_error = true;
                    use_shell_execute = false;
                }

                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = use_shell_execute,
                    WorkingDirectory = workingDir,
                    RedirectStandardOutput = redirect_standard_output,
                    RedirectStandardError = redirect_standard_error,
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