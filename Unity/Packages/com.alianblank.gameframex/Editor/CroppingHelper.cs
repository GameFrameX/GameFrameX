using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace GameFrameX.Editor
{
    public static class CroppingHelper
    {
        [MenuItem("YooAsset/CroppingHelper")]
        static void Run()
        {
            var types = typeof(GameApp).Assembly.GetTypes();
            types = types.OrderBy(m => m.FullName).ToArray();
            StringBuilder stringBuilder = new StringBuilder();
            foreach (var type in types)
            {
                if (type.IsNestedPrivate)
                {
                    continue;
                }

                stringBuilder.AppendLine(" _ = typeof(" + type.FullName.Replace("+", ".") + ");");
            }

            Debug.Log(stringBuilder);
            File.WriteAllText(".\\CroppingHelper.txt", stringBuilder.ToString());
        }
    }
}