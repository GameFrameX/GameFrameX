using Sirenix.OdinInspector;

namespace Luban.Editor
{
    public enum NamingConvertion
    {
        None,

        [LabelText("语言推荐")]
        language_recommend,

        [LabelText("小驼峰 camelCase")]
        camelCase,

        [LabelText("大驼峰 PascalCase")]
        PascalCase,

        [LabelText("下划线 under_scores")]
        under_scores
    }
}