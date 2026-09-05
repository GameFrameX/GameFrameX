using System;
using GameFrameX.ProtoExport;
using Xunit;

namespace ProtoExporterGUI.Tests;

/// <summary>
/// CommentValidator 的契约：
/// 1) 按 CommentValidationLevel 分级校验 message/enum 容器注释与成员字段注释；
/// 2) 缺注释时聚合全部错误后抛 Exception（含 Err_CommentValidationFailed 外壳）；
/// 3) IsValid = false 的字段（Name/Type 缺失）不参与成员校验；
/// 4) level = None 时完全跳过。
/// </summary>
/// <remarks>
/// 异常消息断言只用文件名 / 消息名 / 字段名字面量（Contains），不依赖当前 culture 的文案。
/// </remarks>
public class CommentValidatorTests
{
    private static MessageInfoList NewList()
    {
        return new MessageInfoList
        {
            FileName = "test.proto",
            Module = 1,
            ModuleName = "Test",
        };
    }

    private static MessageInfo CommentedMessage(string name)
    {
        var info = new MessageInfo { Name = name, Description = "消息注释" };
        info.Fields.Add(new MessageMember { Name = "Value", Type = "string", Description = "字段注释" });
        return info;
    }

    /// <summary>
    /// 全部注释齐全_All级静默通过：容器与成员都有注释时 All 级校验不抛异常。
    /// </summary>
    [Fact]
    public void AllCommentsPresent_AllLevel_PassesWithoutThrowing()
    {
        var list = NewList();
        list.Infos.Add(CommentedMessage("ReqDemo"));

        var ex = Record.Exception(() => CommentValidator.Validate(list, CommentValidationLevel.All));

        Assert.Null(ex);
    }

    /// <summary>
    /// 容器缺注释_Container级报错：message 无注释时抛异常，错误携带文件名与消息名。
    /// </summary>
    [Fact]
    public void MissingContainerComment_ContainerLevel_Throws()
    {
        var list = NewList();
        list.Infos.Add(new MessageInfo { Name = "ReqDemo" }); // Description 默认空

        var ex = Assert.Throws<Exception>(() => CommentValidator.Validate(list, CommentValidationLevel.Container));

        Assert.Contains("test.proto", ex.Message);
        Assert.Contains("ReqDemo", ex.Message);
    }

    /// <summary>
    /// 成员缺注释_Member级报错：字段无注释时抛异常，错误携带消息名与字段名。
    /// </summary>
    [Fact]
    public void MissingMemberComment_MemberLevel_Throws()
    {
        var list = NewList();
        var info = new MessageInfo { Name = "ReqDemo", Description = "消息注释" };
        info.Fields.Add(new MessageMember { Name = "Value", Type = "string" }); // Description 默认空
        list.Infos.Add(info);

        var ex = Assert.Throws<Exception>(() => CommentValidator.Validate(list, CommentValidationLevel.Member));

        Assert.Contains("ReqDemo", ex.Message);
        Assert.Contains("Value", ex.Message);
    }

    /// <summary>
    /// None级_完全不校验：即使容器与成员注释全缺也不抛异常。
    /// </summary>
    [Fact]
    public void LevelNone_NeverThrowsEvenWhenCommentsMissing()
    {
        var list = NewList();
        list.Infos.Add(new MessageInfo { Name = "ReqDemo" });

        var ex = Record.Exception(() => CommentValidator.Validate(list, CommentValidationLevel.None));

        Assert.Null(ex);
    }

    /// <summary>
    /// All级_聚合容器与成员错误：两类缺注释同时存在时，一次抛出携带全部错误（换行拼接）。
    /// </summary>
    [Fact]
    public void AllLevel_AggregatesContainerAndMemberErrors()
    {
        var list = NewList();
        var info = new MessageInfo { Name = "ReqDemo" }; // 容器缺注释
        info.Fields.Add(new MessageMember { Name = "Value", Type = "string" }); // 成员缺注释
        list.Infos.Add(info);

        var ex = Assert.Throws<Exception>(() => CommentValidator.Validate(list, CommentValidationLevel.All));

        // 两条错误都被聚合（多错误间用换行拼接）
        Assert.Contains("ReqDemo", ex.Message);
        Assert.Contains("Value", ex.Message);
        Assert.Contains("\n", ex.Message);
    }

    /// <summary>
    /// 无效字段_跳过成员校验：IsValid = false 的字段（Name/Type 缺失，视为占位成员）即使无注释也不报错。
    /// </summary>
    [Fact]
    public void InvalidFields_SkippedFromMemberValidation()
    {
        var list = NewList();
        var info = new MessageInfo { Name = "ReqDemo", Description = "消息注释" };
        info.Fields.Add(new MessageMember()); // Name/Type 均空 → IsValid = false
        list.Infos.Add(info);

        var ex = Record.Exception(() => CommentValidator.Validate(list, CommentValidationLevel.Member));

        Assert.Null(ex);
    }

    /// <summary>
    /// 枚举容器_同样参与校验：enum 条目按 kind = "enum" 纳入容器注释校验。
    /// </summary>
    [Fact]
    public void EnumContainers_ParticipateInValidation()
    {
        var list = NewList();
        list.Infos.Add(new MessageInfo(true) { Name = "ItemKind" }); // enum，Description 默认空

        var ex = Assert.Throws<Exception>(() => CommentValidator.Validate(list, CommentValidationLevel.Container));

        Assert.Contains("ItemKind", ex.Message);
    }

    /// <summary>
    /// Flags语义_All等于Container或Member：等级枚举的位标志组合语义固化。
    /// </summary>
    [Fact]
    public void FlagsSemantics_AllEqualsContainerOrMember()
    {
        Assert.Equal(CommentValidationLevel.Container | CommentValidationLevel.Member, CommentValidationLevel.All);

        Assert.True(CommentValidationLevel.All.HasFlag(CommentValidationLevel.Container));
        Assert.True(CommentValidationLevel.All.HasFlag(CommentValidationLevel.Member));
        Assert.False(CommentValidationLevel.Container.HasFlag(CommentValidationLevel.Member));
        Assert.False(CommentValidationLevel.Member.HasFlag(CommentValidationLevel.Container));
    }
}
