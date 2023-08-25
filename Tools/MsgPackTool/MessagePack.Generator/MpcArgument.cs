using System.Collections.Generic;
using System.Linq;
using System.Text;


public class MpcArgument
{
    public string ClientProtoPath { get; set; }
    public string ClientOutput { get; set; }
    public string ClientFormatNameSpace { get; set; }

    public string ServerProtoPath { get; set; }
    public string ServerNameSpace { get; set; }


    public string ServerOutput { get; set; }
    public string BaseMessageName { get; set; }
    public string ConditionalSymbol { get; set; }
    public string ResolverName = "GeneratedResolver";
    public string Namespace = "MessagePack";
    public bool UseMapMode { get; set; }
    public string MultipleIfDirectiveOutputSymbols { get; set; }
    public bool ServerIsGenerated { get; set; }
    public List<string> NoExportTypes { get; set; }
}