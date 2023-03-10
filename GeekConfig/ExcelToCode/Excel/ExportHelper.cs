using DotLiquid;
using ExcelConverter.Utils;
using NLog;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ExcelToCode.Excel
{
    public class ExportHelper
    {

        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 普通配置表缓存
        /// </summary>
        public static byte[] NormalBuffer = null;

        /// <summary>
        /// 语言包缓存
        /// </summary>
        public static byte[] LanguageBuffer = null;

        /// <summary>
        /// 是否初始化 
        /// </summary>
        private static bool IsInited = false;

        public static void Init()
        {
            if (IsInited)
                return;
            IsInited = true;
            NormalBuffer = new byte[Setting.NormalBufferSize];
            LanguageBuffer = new byte[Setting.LanguageBufferSize];
        }

        public static void Export(ExportType etype, List<string> fileList, bool isAll)
        {
            Init();

            //只有全部导出的时候才清空，导出单个文件不清空
            if (isAll)
            {
                //清空原来的代码目录
                string path = Setting.GetCodePath(etype);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                else
                    FileUtil.ClearDirectory(path);

                //清空原来的Bin目录
                path = Setting.GetBinPath(etype);
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
                else
                    FileUtil.ClearDirectory(path);
            }

            DataMgrInfo mgrInfo = new DataMgrInfo();
            for (int i = 0; i < fileList.Count; i++)
            {
                ExcelReader excelReader = new ExcelReader();
                ExcelPackage package = null;
                List<SheetHeadInfo> headInfos = excelReader.ReadHeadInfo(fileList[i], etype, out package);

                GenBin(headInfos, package, etype);
                //Task.Run(()=> { GenBin(headInfos, package, etype); });
                GenBeanAddContainer(headInfos, etype, mgrInfo);
             

                LogUtil.AddNormalLog(package.File.Name, "导表完成");

                if (package != null)
                    package.Dispose();
            }

            GenGameDataManager(mgrInfo, etype, isAll);
            ExcelToCode.Program.MainForm.ToggleAllBtn(true);
            LogUtil.Add("---------导表完成-------------");
        }

        private static void GenBeanAddContainer(List<SheetHeadInfo> headInfos, ExportType etype, DataMgrInfo mgrInfo)
        {
            string beanPath = Setting.GetCodePath(etype) + @"/Data/Beans/";
            if(!Directory.Exists(beanPath))
                Directory.CreateDirectory(beanPath);
            string containerPath = Setting.GetCodePath(etype) + @"/Data/Containers/";
            if (!Directory.Exists(containerPath))
                Directory.CreateDirectory(containerPath);


            string content = "";
            string templatePath = Setting.GetTemplatePath(etype) + "/Bean.template";
            Template template = Template.Parse(File.ReadAllText(templatePath));
            foreach (var info in headInfos)
            {
                if (info.SheetName == "t_language")
                {
                    string lanTemplatePath = Setting.GetTemplatePath(etype) + "/LanBean.template";
                    Template lanTemplate = Template.Parse(File.ReadAllText(lanTemplatePath));
                    content = lanTemplate.Render(Hash.FromAnonymousObject(info));
                    File.WriteAllText(beanPath + info.BeanClassName + ".cs", content);
                }
                else
                {
                    content = template.Render(Hash.FromAnonymousObject(info));
                    File.WriteAllText(beanPath + info.BeanClassName + ".cs", content);
                }
            }

            templatePath = Setting.GetTemplatePath(etype) + "/Container.template";
            template = Template.Parse(File.ReadAllText(templatePath));
            foreach (var info in headInfos)
            {
                content = template.Render(Hash.FromAnonymousObject(info));
                File.WriteAllText(containerPath + info.ContainerClassName + ".cs", content);
            }

            for (int i = 0; i < headInfos.Count; i++)
            {
                mgrInfo.Containers.Add(headInfos[i].ContainerClassName);
            }
        }

        private static void GenGameDataManager(DataMgrInfo mgrInfo, ExportType etype, bool isAll)
        {
            string path = Setting.GetCodePath(etype) + @"/GameDataManager.cs";
            if (isAll || !File.Exists(path))
            {
                string templatePath = Setting.GetTemplatePath(etype) + "/GameDataManager.template";
                Template template = Template.Parse(File.ReadAllText(templatePath));
                var str = template.Render(Hash.FromAnonymousObject(mgrInfo));
                File.WriteAllText(Setting.GetCodePath(etype) + "GameDataManager.cs", str);
            }
            else
            {
                if (mgrInfo == null || mgrInfo.Containers.Count <= 0)
                    return;
                string containerName = mgrInfo.Containers[0];

                string part1 = string.Format("public {0} {1} = new {2}();", containerName, containerName, containerName);
                string part2 = string.Format("t_containerMap.Add({0}.BinType, {1});", containerName, containerName);
                string part3 = string.Format("LoadOneBean({0}.BinType, forceReload);", containerName);

                string content = File.ReadAllText(path);
                int index = content.IndexOf("@%@%@");
                if (index != -1)
                {
                    if(content.IndexOf(part1) < 0)
                        content = content.Insert(index-6, "\r\n\t\t" + part1);
                }
                else
                {
                    LogUtil.Add("找不到标记位：@%@%@", true);
                }

                index = content.IndexOf("@#@#@");
                if (index != -1)
                {
                    if (content.IndexOf(part2) < 0)
                        content = content.Insert(index-6, "\r\n\t\t\t" + part2);
                }
                else
                {
                    LogUtil.Add("找不到标记位：@#@#@", true);
                }

                index = content.IndexOf("@*@*@");
                if (index != -1)
                {
                    if (content.IndexOf(part3) < 0)
                        content = content.Insert(index-6, "\r\n\t\t\t" + part3);
                }
                else
                {
                    LogUtil.Add("找不到标记位：@*@*@", true);
                }

                File.WriteAllText(Setting.GetCodePath(etype) + "GameDataManager.cs", content);
            }
        }


        private static void GenBin(List<SheetHeadInfo> headInfos, ExcelPackage package, ExportType etype)
        {
            for (int i = 0; i < headInfos.Count; i++)
            {
                SheetHeadInfo headInfo = headInfos[i];
                ExcelWorksheet sheet = package.Workbook.Worksheets[headInfo.SheetId]; //只导出合法表单id的数据 
                //空表没有数据
                if (ExcelReader.DataStartRow > sheet.Dimension.End.Row)
                    continue;
                //首先清空缓冲区
                byte[] byteArr = null;
                if (headInfo.SheetName == "t_language")
                {
                    Array.Clear(LanguageBuffer, 0, LanguageBuffer.Length);
                    byteArr = LanguageBuffer;
                }
                else
                {
                    Array.Clear(NormalBuffer, 0, NormalBuffer.Length);
                    byteArr = NormalBuffer;
                }
                int offset = 0;
                //写入文件头----表名string-字段数量byte-字段类型byte (0:int 1:long 2:string 3:float)
                //XBuffer.WriteString(headInfos[i].SheetName, byteArr, ref offset);
                XBuffer.WriteInt(headInfos[i].Fields.Count, byteArr, ref offset);
                for (int k = 0; k < headInfos[i].Fields.Count; k++)
                {
                    Field field = headInfos[i].Fields[k];
                    switch (field.Datatype)
                    {
                        case DataType.Int:
                        case DataType.TextMult:
                            XBuffer.WriteByte(0, byteArr, ref offset);
                            break;
                        case DataType.Long:
                            XBuffer.WriteByte(1, byteArr, ref offset);
                            break;
                        case DataType.Text:
                        case DataType.String:
                            XBuffer.WriteByte(2, byteArr, ref offset);
                            break;
                        case DataType.Float:
                            XBuffer.WriteByte(3, byteArr, ref offset);
                            break;
                        default:
                            //抛异常
                            break;
                    }


                }
                for (int k = 0; k < headInfo.Fields.Count; k++)
                {
                    var content = "";
                    Field field = headInfo.Fields[k];
                    content = field.Name;
                    if (string.IsNullOrEmpty(content))
                        content = "";
                    content = content.Trim();
                    //处理换行符
                    content = content.Replace(@"\n", "\n");
                    XBuffer.WriteString(content, byteArr, ref offset);
                }

                headInfo.ContentStartOffset = offset;

                //写入数据
                for (int m = ExcelReader.DataStartRow, n = sheet.Dimension.End.Row; m <= n; m++)
                {
                    //为了严格保证有序，遍历List,不遍历dictionary
                    //foreach (KeyValuePair<int, Field> item in headInfos[i].ValidFileds)
                    for (int j = 0; j < headInfos[i].Fields.Count; j++)
                    {
                        int col = headInfos[i].Fields[j].Col;
                        Field field = headInfos[i].Fields[j];

                        var content = "";
                        var obj = sheet.GetValue(m, col);
                        if (obj != null)
                            content = obj.ToString();

                        //排除id为0的数据行
                        if (j == 0 && string.IsNullOrEmpty(content))
                        {
                            break;
                        }

                        switch (field.Datatype)
                        {
                            case DataType.Int:
                            case DataType.TextMult:
                                int intVal = 0;
                                int.TryParse(content, out intVal);
                                XBuffer.WriteInt(intVal, byteArr, ref offset);
                                break;
                            case DataType.Text:
                            case DataType.String:
                                if (string.IsNullOrEmpty(content))
                                    content = "";
                                content = content.Trim();
                                //处理换行符
                                content = content.Replace(@"\n", "\n");
                                XBuffer.WriteString(content, byteArr, ref offset);
                                break;
                            case DataType.Float:
                                float floatVal = 0;
                                float.TryParse(content, out floatVal);
                                XBuffer.WriteFloat(floatVal, byteArr, ref offset);
                                break;
                            case DataType.Long:
                                long longVal = 0;
                                long.TryParse(content, out longVal);
                                XBuffer.WriteLong(longVal, byteArr, ref offset);
                                break;
                            default:
                                //抛异常
                                break;
                        }
                    }
                }
                //将数据写入磁盘
                System.IO.File.WriteAllBytes(Setting.GetBinPath(etype) + headInfo.SheetName + "Bean.bytes", GetValidData(byteArr, offset));
            }
        }

        /// <summary>
        /// 获取Byte数组内有效数据
        /// </summary>
        /// <param name="org"></param>
        /// <param name="validLen"></param>
        /// <returns></returns>
        public static byte[] GetValidData(byte[] org, int validLen)
        {
            if (validLen > org.Length)
            {
                LOGGER.Error("数据异常org:{},valid:{}", org.Length, validLen);
                return org;
            }
            byte[] res = new byte[validLen];
            Array.Copy(org, 0, res, 0, validLen);
            return res;
        }

    }
}
