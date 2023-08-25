// Copyright (c) All contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using ConsoleAppFramework;
using MessagePackCompiler;
using Microsoft.Build.Locator;
using Microsoft.Build.Logging;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessagePack.Generator
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            var instance = MSBuildLocator.RegisterDefaults();

            Console.WriteLine("Geek.MsgPackTool start....");

            //初始化配置信息
            if (!Setting.Init())
            {
                Console.WriteLine("----配置错误，启动失败----");
                return;
            }


            Gen().Wait();
        }

        static async Task Gen()
        {
            MpcArgument mpcArgument = new MpcArgument();
            mpcArgument.ServerProtoPath = Setting.Ins.ServerProtoPath;
            mpcArgument.ServerOutput = Setting.Ins.ServerOutPath;
            mpcArgument.ServerNameSpace = Setting.Ins.ServerNameSpace;
            mpcArgument.ServerIsGenerated = Setting.Ins.ServerIsGenerated;
            mpcArgument.BaseMessageName = Setting.Ins.BaseMessageName;
            mpcArgument.NoExportTypes = Setting.Ins.NoExportList;
            mpcArgument.ClientProtoPath = Setting.Ins.ClientProtoPath;
            mpcArgument.ClientFormatNameSpace = Setting.Ins.ClientFormatNameSpace;
            mpcArgument.ClientOutput = Setting.Ins.ClientOutPath;
            await RunAsync(mpcArgument);
        }

        public static async Task RunAsync(MpcArgument args)
        {
            GeekGenerator.BaseMessage = args.BaseMessageName;
            GeekGenerator.NoExportTypes = new List<string>();
            if (args.NoExportTypes != null)
                GeekGenerator.NoExportTypes.AddRange(args.NoExportTypes);

            Workspace? workspace = null;
            try
            {
                // Client
                Compilation compilation;


                if (Directory.Exists(args.ClientProtoPath))
                {
                    string[]? conditionalSymbols = args.ConditionalSymbol?.Split(',');
                    compilation = await PseudoCompilation.CreateFromDirectoryAsync(args.ClientProtoPath, conditionalSymbols, CancellationToken.None);
                }
                else
                {
                    (workspace, compilation) = await OpenMSBuildProjectAsync(args.ClientProtoPath, CancellationToken.None);
                }

                await new CodeGenerator(x => Console.WriteLine(x), CancellationToken.None)
                    .GenerateFileAsync(
                        compilation,
                        true,
                        args.ClientOutput,
                        args.ServerIsGenerated,
                        args.ResolverName,
                        args.Namespace,
                        args.UseMapMode,
                        args.MultipleIfDirectiveOutputSymbols,
                        null).ConfigureAwait(false);


                // Server 
                if (Directory.Exists(args.ServerProtoPath))
                {
                    string[]? conditionalSymbols = args.ConditionalSymbol?.Split(',');
                    compilation = await PseudoCompilation.CreateFromDirectoryAsync(args.ServerProtoPath, conditionalSymbols, CancellationToken.None);
                }
                else
                {
                    (workspace, compilation) = await OpenMSBuildProjectAsync(args.ServerProtoPath, CancellationToken.None);
                }

                await new CodeGenerator(x => Console.WriteLine(x), CancellationToken.None)
                    .GenerateFileAsync(
                        compilation,
                        false,
                        args.ServerOutput,
                        args.ServerIsGenerated,
                        args.ResolverName,
                        args.Namespace,
                        args.UseMapMode,
                        args.MultipleIfDirectiveOutputSymbols,
                        null).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await Console.Error.WriteLineAsync("error:" + e.Message);
            }
            finally
            {
                //   MSBuildLocator.Unregister();
            }
        }

        static private async Task<(Workspace Workspace, Compilation Compilation)> OpenMSBuildProjectAsync(string projectPath, CancellationToken cancellationToken)
        {
            var workspace = MSBuildWorkspace.Create();
            try
            {
                var logger = new ConsoleLogger(Microsoft.Build.Framework.LoggerVerbosity.Quiet);
                var project = await workspace.OpenProjectAsync(projectPath, logger, null, cancellationToken);
                var compilation = await project.GetCompilationAsync(cancellationToken);
                if (compilation is null)
                {
                    throw new NotSupportedException("The project does not support creating Compilation.");
                }

                return (workspace, compilation);
            }
            catch
            {
                workspace.Dispose();
                throw;
            }
        }
    }
}