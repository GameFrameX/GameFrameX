# GeekConfig
Excel一键导出二进制数据及解析代码，Super Fast，支持Unity3d，IlRuntime，.Netcore , .Net Framework  
API简洁，使用方便，性能高

### Excel填表规则（请参考output/excel下模板进行填写）
1.第一行：CS代表这张表客户端和服务器都会导出（c:client s:server,不填代表cs）  
2.第二行：字段名称，必须以t_开头（非t_开头列不会被导出）  
3.第三行：数据类型，目前支持int（默认可以不填），string(text), long, textmult(多语言处理，代表这个字段会从语言表中读取真正的值)，float  
4.第四行：第一列为表名（主键列，一定会导出），后续的列可以通过填c,s,cs,sc来控制是否需要导出（c:client s:server,不填代表cs）  
5.第五行：字段备注  
6.表单名字必须以t_开头（非t_开头表单不会被导出）,支持多个表单  

### 支持按需加载/启动时全部加载（非线程安全）
GameDataManager.Instance.LoadAll();  
建议:服务器一开始就全部加载，客户端（unity3d）单线程按需加载即可

### 按ID获取数据
var bean = ConfigBean.GetBean<t_globalBean, int>(1020001);  
if(bean != null) Console.WriteLine(bean.t_string_param);

### 获取整个列表
var list = ConfigBean.GetBeanList<t_monsterBean>();  
foreach (var item in list)  
    Console.WriteLine(item.t_name + "-------" + item.t_skill);

### 说明
Configs/config.xml: 可以对工具相关路径进行设置  
Configs/Template: 工程中的client和server模板是一样的，可以根据自己的需求修改
![Image text](https://github.com/leeveel/ExcelToCode/blob/main/Doc/configtool.png)
