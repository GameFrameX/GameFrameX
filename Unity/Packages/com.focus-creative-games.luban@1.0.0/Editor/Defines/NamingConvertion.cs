// using Sirenix.OdinInspector;

namespace Luban.Editor
{
    public enum NamingConvertion
    {
        None,

        /// <summary>
        /// 语言推荐
        /// </summary>
        // [LabelText("语言推荐")]
        language_recommend,

        /// <summary>
        /// 小驼峰 camelCase
        /// </summary>
        // [LabelText("小驼峰 camelCase")]
        camelCase,

        /// <summary>
        /// 大驼峰 PascalCase
        /// </summary>
        // [LabelText("大驼峰 PascalCase")]
        PascalCase,

        /// <summary>
        /// 下划线
        /// </summary>
        // [LabelText("下划线 under_scores")]
        under_scores
    }
}