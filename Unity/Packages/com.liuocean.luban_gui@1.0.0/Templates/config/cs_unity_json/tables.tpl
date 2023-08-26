using Bright.Serialization;
using SimpleJSON;
{{
    name = x.name
    namespace = x.namespace
    tables = x.tables
}}

{{cs_start_name_space_grace x.namespace}} 
   
    public sealed partial class {{name}}
    {
        {{~for table in tables ~}}
        private {{table.full_name}} m_{{table.name}}; 
        {{~if table.comment != '' ~}}
        /// <summary>
        /// {{table.escape_comment}}
        /// </summary>
        {{~end~}}               
        public {{table.full_name}} {{table.name}} 
        {
            get
            { 
                if (m_{{table.name}}==null)
                {
                    m_{{table.name}} = new {{table.full_name}}(_loader("{{table.output_data_file}}")); 
                    tables.Add("{{table.full_name}}", m_{{table.name}});
                    m_{{table.name}}.Resolve(tables); 
                    m_{{table.name}}.TranslateText(_translator); 
                }
                return m_{{table.name}};
            } 
        }
        
        {{~end~}}
    
        private System.Collections.Generic.Dictionary<string,object> tables = new System.Collections.Generic.Dictionary<string, object>();
        private System.Func<string,JSONNode> _loader;
        private System.Func<string, string, string> _translator;
        public {{name}}(System.Func<string, JSONNode> loader, System.Func<string, string, string> translator = null)
        {
            _loader = loader;
            _translator = translator;
            tables.Clear();
            /*
            {{~for table in tables ~}}
            {{table.name}} = new {{table.full_name}}(loader("{{table.output_data_file}}")); 
            tables.Add("{{table.full_name}}", {{table.name}});
            {{~end~}}
            PostInit();
    
            {{~for table in tables ~}}
            {{table.name}}.Resolve(tables); 
            {{~end~}}
            
            PostResolve();
            */
        }
        /*
        public void TranslateText(System.Func<string, string, string> translator)
        {
            {{~for table in tables ~}}
            {{table.name}}.TranslateText(translator); 
            {{~end~}}
        }
        
        partial void PostInit();
        partial void PostResolve();
        */
    }
{{cs_end_name_space_grace x.namespace}}