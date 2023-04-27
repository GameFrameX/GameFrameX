using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using UnityEngine;

namespace ExcelExporter.Editor
{
    public enum ConfigType
    {
        c = 0,
        s = 1,
        cs = 2,
    }

    class HeadInfo
    {
        public string FieldCS;
        public string FieldDesc;
        public string FieldName;
        public string FieldType;
        public int FieldIndex;

        public HeadInfo(string cs, string desc, string name, string type, int index)
        {
            this.FieldCS = cs;
            this.FieldDesc = desc;
            this.FieldName = name;
            this.FieldType = type;
            this.FieldIndex = index;
        }
    }

    class Table
    {
        public bool C;
        public bool S;
        public int Index;
        public Dictionary<string, HeadInfo> HeadInfos = new Dictionary<string, HeadInfo>();
    }

    public static class ExcelExporter
    {
        private static string template;

        private const string ClientClassNameSpace = "Hotfix.Config";

        // 服务端因为机器人的存在必须包含客户端所有配置，所以单独的c字段没有意义,单独的c就表示cs
        private const string ServerClassNameSpace = "Server.Config";

        private const string ClientClassDir = "../Unity/Assets/Hotfix/Config/Generate";

        // 服务端因为机器人的存在必须包含客户端所有配置，所以单独的c字段没有意义,单独的c就表示cs
        private const string ServerClassDir = "../Server/Server.Config/Config";

        private const string CSClassDir = "../Config";

        private const string excelDir = "../Config/Excel/";

        private const string ClientJsonDir = "../Unity/Assets/Bundles/Config";
        private const string ServerJsonDir = "../Server/Server.Config/Json";

        // private const string clientProtoDir = "../Unity/Assets/Bundles/Config";
        //
        // private const string serverProtoDir = "../Config/Excel/{0}/{1}";
        // private static Assembly[] configAssemblies = new Assembly[3];

        private static Dictionary<string, Table> tables = new Dictionary<string, Table>();
        private static Dictionary<string, ExcelPackage> packages = new Dictionary<string, ExcelPackage>();

        private static Table GetTable(string protoName)
        {
            if (!tables.TryGetValue(protoName, out var table))
            {
                table = new Table();
                tables[protoName] = table;
            }

            return table;
        }

        public static ExcelPackage GetPackage(string filePath)
        {
            if (!packages.TryGetValue(filePath, out var package))
            {
                using (Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    package = new ExcelPackage(stream);
                    packages[filePath] = package;
                }
            }

            return package;
        }

        public static void Export()
        {
            try
            {
                //防止编译时裁剪掉protobuf
                template = File.ReadAllText("./Packages/com.alianblank.excelexporter@1.0.0/Template.txt");

                if (Directory.Exists(ClientClassDir))
                {
                    Directory.Delete(ClientClassDir, true);
                }

                if (Directory.Exists(ServerClassDir))
                {
                    Directory.Delete(ServerClassDir, true);
                }

                var files = Directory.GetFiles(excelDir, "*.*", SearchOption.AllDirectories);
                foreach (string path in files)
                {
                    string fileName = Path.GetFileName(path);
                    if (!fileName.EndsWith(".xlsx") || fileName.StartsWith("~$") || fileName.Contains("#"))
                    {
                        continue;
                    }

                    string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                    string fileNameWithoutCS = fileNameWithoutExtension;
                    string cs = "cs";
                    if (fileNameWithoutExtension.Contains("@"))
                    {
                        string[] ss = fileNameWithoutExtension.Split('@');
                        fileNameWithoutCS = ss[0];
                        cs = ss[1];
                    }

                    if (cs == "")
                    {
                        cs = "cs";
                    }

                    ExcelPackage p = GetPackage(Path.GetFullPath(path));

                    string protoName = fileNameWithoutCS;
                    if (fileNameWithoutCS.Contains('_'))
                    {
                        protoName = fileNameWithoutCS.Substring(0, fileNameWithoutCS.LastIndexOf('_'));
                    }

                    Table table = GetTable(protoName);

                    if (cs.Contains("c"))
                    {
                        table.C = true;
                    }

                    if (cs.Contains("s"))
                    {
                        table.S = true;
                    }

                    ExportExcelClass(p, protoName, table);
                }

                foreach (var kv in tables)
                {
                    if (kv.Value.C)
                    {
                        ExportClass(kv.Key, kv.Value.HeadInfos, ConfigType.c);
                    }

                    if (kv.Value.S)
                    {
                        ExportClass(kv.Key, kv.Value.HeadInfos, ConfigType.s);
                    }

                    ExportClass(kv.Key, kv.Value.HeadInfos, ConfigType.cs);
                }

                // 动态编译生成的配置代码
                // configAssemblies[(int) ConfigType.c] = DynamicBuild(ConfigType.c);
                // configAssemblies[(int) ConfigType.s] = DynamicBuild(ConfigType.s);
                // configAssemblies[(int) ConfigType.cs] = DynamicBuild(ConfigType.cs);

                var excels = Directory.GetFiles(excelDir, "*.xlsx");

                foreach (string path in excels)
                {
                    ExportExcel(path);
                }

                // if (Directory.Exists(clientProtoDir))
                // {
                //     Directory.Delete(clientProtoDir, true);
                // }

                // CopyDirectory("../Config/Excel/c", clientProtoDir);

                // Debug.Log("Export Excel Sucess!");
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
            finally
            {
                tables.Clear();
                foreach (var kv in packages)
                {
                    kv.Value.Dispose();
                }

                packages.Clear();
            }
        }

        /// <summary>
        /// 目录复制
        /// </summary>
        /// <param name="srcDir"></param>
        /// <param name="tgtDir"></param>
        /// <exception cref="Exception"></exception>
        public static void CopyDirectory(string srcDir, string tgtDir)
        {
            DirectoryInfo source = new DirectoryInfo(srcDir);
            DirectoryInfo target = new DirectoryInfo(tgtDir);

            if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
            {
                throw new Exception("父目录不能拷贝到子目录！");
            }

            if (!source.Exists)
            {
                return;
            }

            if (!target.Exists)
            {
                target.Create();
            }

            FileInfo[] files = source.GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                File.Copy(files[i].FullName, Path.Combine(target.FullName, files[i].Name), true);
            }

            DirectoryInfo[] dirs = source.GetDirectories();

            for (int j = 0; j < dirs.Length; j++)
            {
                CopyDirectory(dirs[j].FullName, Path.Combine(target.FullName, dirs[j].Name));
            }
        }

        private static void ExportExcel(string path)
        {
            string dir = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);
            string relativePath = fileName.Substring(0, fileName.IndexOf('@')); //Path.GetRelativePath(excelDir, dir);
            if (!fileName.EndsWith(".xlsx") || fileName.StartsWith("~$") || fileName.Contains("#"))
            {
                return;
            }

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string fileNameWithoutCs = fileNameWithoutExtension;
            string cs = "cs";
            if (fileNameWithoutExtension.Contains("@"))
            {
                string[] ss = fileNameWithoutExtension.Split('@');
                fileNameWithoutCs = ss[0];
                cs = ss[1];
            }

            if (cs == "")
            {
                cs = "cs";
            }

            string protoName = fileNameWithoutCs;
            if (fileNameWithoutCs.Contains('_'))
            {
                protoName = fileNameWithoutCs.Substring(0, fileNameWithoutCs.LastIndexOf('_'));
            }

            Table table = GetTable(protoName);

            ExcelPackage p = GetPackage(Path.GetFullPath(path));

            if (cs.Contains("c"))
            {
                ExportExcelJson(p, fileNameWithoutCs, table, ConfigType.c, relativePath);
                // ExportExcelProtobuf(ConfigType.c, protoName, relativePath);
            }

            if (cs.Contains("s"))
            {
                ExportExcelJson(p, fileNameWithoutCs, table, ConfigType.s, relativePath);
                // ExportExcelProtobuf(ConfigType.s, protoName, relativePath);
            }

            ExportExcelJson(p, fileNameWithoutCs, table, ConfigType.cs, relativePath);
            // ExportExcelProtobuf(ConfigType.cs, protoName, relativePath);
        }

        // private static string GetProtoDir(ConfigType configType, string relativeDir)
        // {
        //     return string.Format(serverProtoDir, configType.ToString(), relativeDir);
        // }

        // private static Assembly GetAssembly(ConfigType configType)
        // {
        //     return configAssemblies[(int) configType];
        // }

        private static string GetClassDir(ConfigType configType)
        {
            switch (configType)
            {
                case ConfigType.c:
                    return ClientClassDir;
                case ConfigType.s:
                    return ServerClassDir;
                default:
                    return CSClassDir;
            }
        }

        #region 导出class

        static void ExportExcelClass(ExcelPackage p, string name, Table table)
        {
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                ExportSheetClass(worksheet, table);
            }
        }

        static void ExportSheetClass(ExcelWorksheet worksheet, Table table)
        {
            const int row = 2;
            for (int col = 3; col <= worksheet.Dimension.End.Column; ++col)
            {
                if (worksheet.Name.StartsWith("#"))
                {
                    continue;
                }

                string fieldName = worksheet.Cells[row + 2, col].Text.Trim();
                if (fieldName == "")
                {
                    continue;
                }

                if (table.HeadInfos.ContainsKey(fieldName))
                {
                    continue;
                }

                string fieldCs = worksheet.Cells[row, col].Text.Trim().ToLower();
                if (fieldCs.Contains("#"))
                {
                    table.HeadInfos[fieldName] = null;
                    continue;
                }

                if (fieldCs == "")
                {
                    fieldCs = "cs";
                }

                if (table.HeadInfos.TryGetValue(fieldName, out var oldClassField))
                {
                    if (oldClassField.FieldCS != fieldCs)
                    {
                        Debug.Log($"field cs not same: {worksheet.Name} {fieldName} oldcs: {oldClassField.FieldCS} {fieldCs}");
                    }

                    continue;
                }

                string fieldDesc = worksheet.Cells[row + 1, col].Text.Trim();
                string fieldType = worksheet.Cells[row + 3, col].Text.Trim();

                table.HeadInfos[fieldName] = new HeadInfo(fieldCs, fieldDesc, fieldName, fieldType, ++table.Index);
            }
        }

        static void ExportClass(string protoName, Dictionary<string, HeadInfo> classField, ConfigType configType)
        {
            string dir = GetClassDir(configType);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            string exportPath = Path.Combine(dir, $"{protoName}.cs");

            using (FileStream txt = new FileStream(exportPath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(txt))
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var kv in classField)
                    {
                        if (kv.Value == null)
                        {
                            continue;
                        }

                        if (configType != ConfigType.cs && !kv.Value.FieldCS.Contains(configType.ToString()))
                        {
                            continue;
                        }

                        sb.AppendLine();
                        sb.Append($"\t\t/// <summary>\n");
                        sb.Append($"\t\t/// {kv.Value.FieldDesc}\n");
                        sb.Append($"\t\t/// </summary>\n");
                        sb.Append($"\t\t[ProtoMember({kv.Value.FieldIndex})]\n");
                        string fieldType = kv.Value.FieldType;
                        sb.Append($"\t\tpublic {fieldType} {kv.Value.FieldName} {{ get; set; }}\n");
                    }

                    string content = template.Replace("(ConfigName)", protoName).Replace(("(Fields)"), sb.ToString());

                    if (configType == ConfigType.c)
                    {
                        content = content.Replace("(NameSpace)", ClientClassNameSpace);
                    }
                    else
                    {
                        content = content.Replace("(NameSpace)", ServerClassNameSpace);
                    }

                    sw.Write(content);
                }
            }
        }

        #endregion

        #region 导出json

        static void ExportExcelJson(ExcelPackage p, string name, Table table, ConfigType configType, string relativeDir)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{\"list\":[\n");
            foreach (ExcelWorksheet worksheet in p.Workbook.Worksheets)
            {
                if (worksheet.Name.StartsWith("#"))
                {
                    continue;
                }

                ExportSheetJson(worksheet, name, table.HeadInfos, configType, sb);
            }

            sb.Append("]}\n");

            string dir = ClientJsonDir; // string.Format(jsonDir, relativeDir);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!Directory.Exists(ServerJsonDir))
            {
                Directory.CreateDirectory(ServerJsonDir);
            }

            string jsonPath = Path.Combine(dir, $"{name}.json");
            using (FileStream txt = new FileStream(jsonPath, FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(txt))
                {
                    sw.Write(sb.ToString());
                }
            }

            string serverJsonPath = Path.Combine(ServerJsonDir, $"{name}.json");
            File.Copy(jsonPath, serverJsonPath, true);
        }

        static void ExportSheetJson(ExcelWorksheet worksheet, string name,
            Dictionary<string, HeadInfo> classField, ConfigType configType, StringBuilder sb)
        {
            string configTypeStr = configType.ToString();
            List<string> fieldList = new List<string>();
            List<string> lineList = new List<string>();
            lineList.Clear();
            StringBuilder stringBuilder = new StringBuilder();
            for (int row = 6; row <= worksheet.Dimension.End.Row; ++row)
            {
                fieldList.Clear();
                stringBuilder.Clear();
                string prefix = worksheet.Cells[row, 2].Text.Trim();
                if (prefix.Contains("#"))
                {
                    continue;
                }

                if (prefix == "")
                {
                    prefix = "cs";
                }

                if (configType != ConfigType.cs && !prefix.Contains(configTypeStr))
                {
                    continue;
                }

                if (worksheet.Cells[row, 3].Text.Trim() == "")
                {
                    continue;
                }

                stringBuilder.Append("{");
                // sb.Append($"\"_t\":\"{name}\"");

                for (int col = 3; col <= worksheet.Dimension.End.Column; ++col)
                {
                    string fieldName = worksheet.Cells[4, col].Text.Trim();
                    if (!classField.ContainsKey(fieldName))
                    {
                        continue;
                    }

                    HeadInfo headInfo = classField[fieldName];

                    if (headInfo == null)
                    {
                        continue;
                    }

                    if (configType != ConfigType.cs && !headInfo.FieldCS.Contains(configTypeStr))
                    {
                        continue;
                    }

                    string fieldN = headInfo.FieldName;
                    if (fieldN == "Id")
                    {
                        fieldN = "_id";
                    }

                    fieldList.Add($"\"{fieldN}\":{Convert(headInfo.FieldType, worksheet.Cells[row, col].Text.Trim())}");
                }

                stringBuilder.Append(string.Join(",", fieldList.ToArray()));
                stringBuilder.Append("}\n");
                lineList.Add(stringBuilder.ToString());
            }

            sb.Append(string.Join(",", lineList.ToArray()));
        }

        private static string Convert(string type, string value)
        {
            switch (type)
            {
                case "uint[]":
                case "int[]":
                case "int32[]":
                case "long[]":
                    return $"[{value}]";
                case "string[]":
                case "int[][]":
                    return $"[{value}]";
                case "int":
                case "uint":
                case "int32":
                case "int64":
                case "long":
                case "float":
                case "double":
                    if (value == "")
                    {
                        return "0";
                    }

                    return value;
                case "string":
                    value = value.Replace("\\", "\\\\");
                    value = value.Replace("\"", "\\\"");
                    return $"\"{value}\"";
                default:
                    throw new Exception($"不支持此类型: {type}");
            }
        }

        #endregion
    }
}