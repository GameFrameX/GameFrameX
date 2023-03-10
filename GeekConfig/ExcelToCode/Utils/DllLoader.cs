using System;
using System.Reflection;

namespace ExcelConverter.Utils
{
    public class DllLoader
    {
        public static void Load()
        {
            //https://www.cnblogs.com/lip-blog/p/7365942.html
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                String resourceName = "ExcelConverter.libs." + new AssemblyName(args.Name).Name + ".dll";
                using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
                {
                    Byte[] assemblyData = new Byte[stream.Length];
                    stream.Read(assemblyData, 0, assemblyData.Length);
                    return Assembly.Load(assemblyData);
                }
            };
        }
    }
}
