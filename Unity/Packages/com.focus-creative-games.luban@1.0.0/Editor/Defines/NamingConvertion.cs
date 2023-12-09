// using Sirenix.OdinInspector;

namespace Luban.Editor
{
    /// <summary>
    /// 命名转换方式
    /// </summary>
    public enum NamingConvertion
    {
        None,

        /// <summary>
        /// 语言推荐
        /// </summary>
        language_recommend,

        /// <summary>
        /// 小驼峰 camelCase
        /// </summary>
        camelCase,

        /// <summary>
        /// 大驼峰 PascalCase
        /// </summary>
        PascalCase,

        /// <summary>
        /// 下划线
        /// </summary>
        under_scores
    }
}