//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using GameFrameX;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GameFrameX.Editor
{
    /// <summary>
    /// 类型相关的实用函数。
    /// </summary>
    internal static class Type
    {
        private static readonly string[] RuntimeAssemblyNames = Utility.Assembly.GetAssemblies().Where(m => !m.FullName.Contains("Editor")).Select(m => m.FullName).ToArray();

        private static readonly string[] RuntimeOrEditorAssemblyNames = Utility.Assembly.GetAssemblies().Select(m => m.FullName).ToArray();

        /// <summary>
        /// 获取配置路径。
        /// </summary>
        /// <typeparam name="T">配置类型。</typeparam>
        /// <returns>配置路径。</returns>
        internal static string GetConfigurationPath<T>() where T : ConfigPathAttribute
        {
            foreach (var type in Utility.Assembly.GetTypes())
            {
                if (!type.IsAbstract || !type.IsSealed)
                {
                    continue;
                }

                foreach (var fieldInfo in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (fieldInfo.FieldType == typeof(string) && fieldInfo.IsDefined(typeof(T), false))
                    {
                        return (string)fieldInfo.GetValue(null);
                    }
                }

                foreach (var propertyInfo in type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    if (propertyInfo.PropertyType == typeof(string) && propertyInfo.IsDefined(typeof(T), false))
                    {
                        return (string)propertyInfo.GetValue(null, null);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 在运行时程序集中获取指定基类的所有子类的名称。
        /// </summary>
        /// <param name="typeBase">基类类型。</param>
        /// <returns>指定基类的所有子类的名称。</returns>
        internal static string[] GetRuntimeTypeNames(System.Type typeBase)
        {
            return GetTypeNames(typeBase, RuntimeAssemblyNames);
        }

        /// <summary>
        /// 在运行时或编辑器程序集中获取指定基类的所有子类的名称。
        /// </summary>
        /// <param name="typeBase">基类类型。</param>
        /// <returns>指定基类的所有子类的名称。</returns>
        internal static string[] GetRuntimeOrEditorTypeNames(System.Type typeBase)
        {
            return GetTypeNames(typeBase, RuntimeOrEditorAssemblyNames);
        }

        private static string[] GetTypeNames(System.Type typeBase, string[] assemblyNames)
        {
            var typeNames = new List<string>();
            foreach (var assemblyName in assemblyNames)
            {
                Assembly assembly = null;
                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                catch
                {
                    continue;
                }

                if (assembly == null)
                {
                    continue;
                }

                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.IsClass && !type.IsAbstract && typeBase.IsAssignableFrom(type))
                    {
                        typeNames.Add(type.FullName);
                    }
                }
            }

            typeNames.Sort();
            return typeNames.ToArray();
        }
    }
}