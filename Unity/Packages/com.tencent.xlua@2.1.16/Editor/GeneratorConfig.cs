using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CSObjectWrapEditor
{
    public static class GeneratorConfig
    {
#if XLUA_GENERAL
        public static string common_path = "./Gen/";
#else
        public static string common_path = Application.dataPath + "/XLua/Gen/";
#endif

        static GeneratorConfig()
        {
            foreach(var type in (from type in XLua.Utils.GetAllTypes()
                        where type.IsAbstract && type.IsSealed
                        select type))
            {
                foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (field.FieldType == typeof(string) && field.IsDefined(typeof(GenPathAttribute), false))
                    {
                        common_path = field.GetValue(null) as string;
                        if (!common_path.EndsWith("/"))
                        {
                            common_path = common_path + "/";
                        }
                    }
                }

                foreach (var prop in type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (prop.PropertyType == typeof(string) && prop.IsDefined(typeof(GenPathAttribute), false))
                    {
                        common_path = prop.GetValue(null, null) as string;
                        if (!common_path.EndsWith("/"))
                        {
                            common_path = common_path + "/";
                        }
                    }
                }
            }
        }
    }
}