using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using ExcelConverter.Utils;
using System.Drawing;

namespace ExcelToCode.Excel
{
    public class ExcelReader
    {

        private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// 导出类型行号
        /// </summary>
        public static int ExportTypeRow = 1;
        /// <summary>
        /// 字段名字行号
        /// </summary>
        public static int FieldNameRow = 2;
        /// <summary>
        /// 字段数据类型行号
        /// </summary>
        public static int FieldTypeRow = 3;
        /// <summary>
        /// 字段导出类型行号
        /// </summary>
        public static int FieldExportTypeRow = 4;
        /// <summary>
        /// 字段描述行号
        /// </summary>
        public static int FieldDescRow = 5;
        /// <summary>
        /// 字段描述行号
        /// </summary>
        public static int DataStartRow = 6;
        /// <summary>
        /// 主键列号
        /// </summary>
        public static int PrimaryKeyCol = 1;

        /// <summary>
        /// 表头信息（ 用生成代码，只会返回合法的表单）
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public List<SheetHeadInfo> ReadHeadInfo(string filePath, ExportType exportType, out ExcelPackage outPackage)
        {
            List<SheetHeadInfo> res = new List<SheetHeadInfo>();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            ExcelPackage package = new ExcelPackage(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            package.File = new FileInfo(filePath);
            outPackage = package;
            List<int> validList = IsLegal(package, exportType);
            if (validList != null && validList.Count > 0)
            {
                for (int i = 0; i < validList.Count; ++i)
                {
                    ExcelWorksheet sheet = package.Workbook.Worksheets[validList[i]];
                    SheetHeadInfo headInfo = new SheetHeadInfo();
                    //记录表单id
                    headInfo.SheetId = validList[i];
                    //表单名字
                    headInfo.SheetName = StringUtils.Trim(sheet.Name);   //去除空格
                    headInfo.BeanClassName = headInfo.SheetName + "Bean";
                    headInfo.ContainerClassName = headInfo.SheetName + "Container";

                    //导出类型
                    string etype = Obj2String(sheet.GetValue(ExportTypeRow, PrimaryKeyCol));
                    headInfo.Etype = GetExportType(etype);

                    //表单名字描述
                    string sheetDesc = Obj2String(sheet.GetValue(FieldExportTypeRow, PrimaryKeyCol));
                    if (sheetDesc == null)
                        sheetDesc = "";
                    else
                        sheetDesc = StringUtils.Trim(sheetDesc);  //去除空格
                    headInfo.SheetNameDesc = sheetDesc;

                    //获取该表主键类型 
                    string keyStr = Obj2String(sheet.GetValue(FieldTypeRow, PrimaryKeyCol));
                    if (string.IsNullOrEmpty(keyStr))
                        keyStr = "int";
                    if (DataType.IsLegal(keyStr))
                        headInfo.PrimaryKeyType = DataType.GetTrueTyped(keyStr);

                    //收集有效字段
                    for (int j = sheet.Dimension.Start.Column, k = sheet.Dimension.End.Column; j <= k; j++)
                    {
                        bool needExport = false;
                        if (j == sheet.Dimension.Start.Column)
                        {
                            needExport = true;
                        }
                        else
                        {
                            string estr = Obj2String(sheet.GetValue(FieldExportTypeRow, j));
                            ExportType exType = GetExportType(estr);
                            if (exType == ExportType.Both || exType == exportType)
                            {
                                needExport = true;
                            }
                            else if (exType == ExportType.Unknown)
                            {
                                LogUtil.AddIgnoreLog(package.File.Name, sheet.Name, string.Format("未知的导出类型{0}行{1}列[{2}]", FieldNameRow, j, estr));
                                continue;
                            }
                        }
                        string str = Obj2String(sheet.GetValue(FieldNameRow, j));
                        if (str != null && str.StartsWith("t_") && needExport)
                        {
                            Field field = new Field();
                            field.Name = StringUtils.Trim(str);     //去除空格
                            field.Col = j;
                            field.Row = FieldNameRow;
                            if (!headInfo.ValidFiledsName.ContainsKey(field.Name))
                            {
                                headInfo.AddFiled(j, field);
                                headInfo.ValidFiledsName.Add(field.Name, field.Name);
                            }
                            else
                            {
                                LogUtil.AddIgnoreLog(package.File.Name, sheet.Name, string.Format("重复的字段名{0}行{1}列[{2}]", FieldNameRow, j, field.Name));
                                //若需跳过本张表，考虑使用break
                                continue;
                            }
                        }
                    }

                    //收集有效字段数据类型
                    for (int j = sheet.Dimension.Start.Column, k = sheet.Dimension.End.Column; j <= k; j++)
                    {
                        if (headInfo.ValidFileds.ContainsKey(j))
                        {
                            string str = Obj2String(sheet.GetValue(FieldTypeRow, j));
                            Field field = headInfo.ValidFileds[j];
                            if (field != null)
                            {
                                if (DataType.IsLegal(str))
                                {
                                    field.Datatype = DataType.GetTrueTyped(str); 
                                }
                                else
                                {
                                    LOGGER.Error("未知的数据类型{}行{}列", FieldTypeRow, j);
                                    //throw new DataIllegalException(string.Format("未知的数据类型{0}行{1}列", FieldTypeRow, j));
                                    LogUtil.AddIgnoreLog(package.File.Name, sheet.Name, string.Format("错误数据类型{0}行{1}列", FieldTypeRow, j));
                                    //若需跳过本张表，考虑使用break
                                    continue;
                                }
                            }
                        }
                    }

                    //收集有效字段描述
                    for (int j = sheet.Dimension.Start.Column, k = sheet.Dimension.End.Column; j <= k; j++)
                    {
                        if (headInfo.ValidFileds.ContainsKey(j))
                        {
                            //string desc = Obj2String(sheet.GetValue(FieldDescRow, j));
                            //if (desc == null)
                            //    desc = "";
                            //headInfo.AddFiledDesc(j, desc);
                            string str = Obj2String(sheet.GetValue(FieldDescRow, j));
                            Field field = headInfo.ValidFileds[j];
                            if (field != null)
                            {
                                if (str != null)
                                    str = StringUtils.Trim(str); //去除空格
                                field.Desc = str;
                            }
                        }
                    }

                    res.Add(headInfo);
                }
            }
            return res;
        }

        public string Obj2String(object obj)
        {
            string str = "";
            if (obj != null)
                str = obj.ToString();
            return str;
        }

        /// <summary>
        /// 获取导出类型
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private ExportType GetExportType(string type)
        {
            //不填的时候默认为都导出
            if (string.IsNullOrEmpty(type))
                type = "cs";
            //全部转换成小写
            type = type.ToLower().Trim();
            if (type.Equals("cs") || type.Equals("sc"))
                return ExportType.Both;
            else if (type.Contains("c"))
                return ExportType.Client;
            else if (type.Contains("s"))
                return ExportType.Server;
            return ExportType.Unknown;
        }
    
        /// <summary>
        /// 表格是否合法
        /// </summary>
        /// <returns>返回合法的表单id</returns>
        private List<int> IsLegal(ExcelPackage package, ExportType exportType)
        {
            List<int> list = new List<int>();
            if (package != null 
                && package.Workbook != null 
                && package.Workbook.Worksheets != null 
                && package.Workbook.Worksheets.Count > 0)
            {
                for (int i = 0; i < package.Workbook.Worksheets.Count; ++i)
                {
                    ExcelWorksheet sheet = package.Workbook.Worksheets[i];
                    //表名字必须以t_开头
                    if (!sheet.Name.StartsWith("t_"))
                    {
                        LogUtil.AddIgnoreLog(package.File.Name, sheet.Name, "表名须以t_开头");
                        continue;
                    }
                    //从1行开始
                    if (sheet.Dimension.Start.Row != 1)
                    {
                        LogUtil.AddIgnoreLog(package.File.Name, sheet.Name, "需从1行开始");
                        continue;
                    }

                    //从1列开始
                    if (sheet.Dimension.Start.Column != 1)
                    {
                        LogUtil.AddIgnoreLog(package.File.Name, sheet.Name, "需从1列开始");
                        continue;
                    }
                    //至少有5行
                    if (sheet.Dimension.End.Row < 5)
                    {
                        LogUtil.AddIgnoreLog(package.File.Name, sheet.Name, "至少有5行");
                        continue;
                    }

                    //至少有2列
                    if (sheet.Dimension.End.Column < 2)
                    {
                        LogUtil.AddIgnoreLog(package.File.Name, sheet.Name, "至少有2列");
                        continue;
                    }

                    //导出类型
                    string etype = Obj2String(sheet.GetValue(ExportTypeRow, PrimaryKeyCol));
                    var Etype = GetExportType(etype);
                    if (Etype != ExportType.Both && Etype != exportType)
                    {
                        LogUtil.AddIgnoreLog(package.File.Name, sheet.Name, "此表不需要在" + exportType.ToString() + "中导出");
                        continue;
                    }

                    //第2行至少有两列数据合法
                    int validColumnCount = 0;
                    for (int j = sheet.Dimension.Start.Column, k = sheet.Dimension.End.Column; j <= k; j++)
                    {
                        string str = Obj2String(sheet.GetValue(FieldNameRow, j));
                        if (str != null && str.StartsWith("t_"))
                        {
                            validColumnCount++;
                            if (validColumnCount > 2)
                                break;
                        }
                    }
                    if (validColumnCount < 2)
                    {
                        LogUtil.AddIgnoreLog(package.File.Name, sheet.Name, "第2行至少有两列数据字段");
                        continue;
                    }

                    if (!IsExportCountLegal(Etype, sheet))
                    {
                        LogUtil.AddIgnoreLog(package.File.Name, sheet.Name, "导出字段个数需大于2");
                        continue;
                    }
                        
                    //合法表单 
                    list.Add(i);
                }
            }
            return list;
        }


        public bool IsExportCountLegal(ExportType etype, ExcelWorksheet sheet)
        {
            int ccount = 0;
            int scount = 0;
            switch (etype)
            {
                case ExportType.Unknown:
                    return false;
                case ExportType.Server:
                    for (int j = sheet.Dimension.Start.Column, k = sheet.Dimension.End.Column; j <= k; j++)
                    {
                        //第一列必须全部导出
                        if (j == 1)
                        {
                            ccount++;
                            scount++;
                            continue;
                        }
                        string str = Obj2String(sheet.GetValue(FieldExportTypeRow, j));
                        if (string.IsNullOrEmpty(str))
                            str = "cs";
                        str = str.ToLower();
                        if (str.Contains("s"))
                        {
                            scount++;
                        }
                    }
                    return scount >= 2;
                case ExportType.Client:
                    for (int j = sheet.Dimension.Start.Column, k = sheet.Dimension.End.Column; j <= k; j++)
                    {
                        //第一列必须全部导出
                        if (j == 1)
                        {
                            ccount++;
                            scount++;
                            continue;
                        }
                        string str = Obj2String(sheet.GetValue(FieldExportTypeRow, j));
                        if (string.IsNullOrEmpty(str))
                            str = "cs";
                        str = str.ToLower();
                        if (str.Contains("c"))
                        {
                            ccount++;
                        }
                    }
                    return ccount >= 2;
                case ExportType.Both:
                    for (int j = sheet.Dimension.Start.Column, k = sheet.Dimension.End.Column; j <= k; j++)
                    {
                        //第一列必须全部导出
                        if (j == 1)
                        {
                            ccount++;
                            scount++;
                            continue;
                        }
                        string str = Obj2String(sheet.GetValue(FieldExportTypeRow, j));
                        if (string.IsNullOrEmpty(str))
                            str = "cs";
                        str = str.ToLower();
                        if (str.Contains("cs") || str.Contains("sc"))
                        {
                            ccount++;
                            scount++;
                        }
                        else if (str.Contains("c"))
                        {
                            ccount++;
                        }
                        else if (str.Contains("s"))
                        {
                            scount++;
                        }
                    }
                    return ccount >= 2 && scount >= 2;
                default:
                    return false;
            }
        }


    }
}
