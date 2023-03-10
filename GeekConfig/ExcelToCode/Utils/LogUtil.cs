
using NLog;
using System.Text;
using System.Windows.Forms;

public class LogUtil
{

    private static readonly NLog.Logger LOGGER = LogManager.GetCurrentClassLogger();

    private static TextBox normalLogTb;

    private static TextBox errorLogTb;

    public static void Init(TextBox tb, TextBox errTb)
    {
        normalLogTb = tb;
        errorLogTb = errTb;
    }

    public static void Add(string log, bool isErr = false)
    {
        if (isErr)
        {
            errorLogTb.AppendText(log + "\r\n");
            LOGGER.Error(log);
        }
        else
        {
            normalLogTb.AppendText(log + "\r\n");
            LOGGER.Info(log);
        }
    }

    public static void AddIgnoreLog(string fileName, string sheetName, string reason)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        sb.Append(fileName);
        sb.Append("]--");
        sb.Append("[");
        sb.Append(sheetName);
        sb.Append("]--");
        sb.Append(reason);
        sb.Append("--被忽略");
        Add(sb.ToString(), true);
    }

    public static void AddNormalLog(string fileName, string msg)
    {
        StringBuilder sb = new StringBuilder();
        sb.Append("[");
        sb.Append(fileName);
        sb.Append("]--");
        sb.Append(msg);
        Add(sb.ToString());
    }



}