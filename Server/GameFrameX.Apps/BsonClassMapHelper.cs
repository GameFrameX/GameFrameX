// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//  
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//  
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
//  
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes and liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//  
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using GameFrameX.Foundation.Logger;

namespace GameFrameX.Apps;

public class DictionaryRepresentationConvention : ConventionBase, IMemberMapConvention
{
    private readonly DictionaryRepresentation _dictionaryRepresentation;

    public DictionaryRepresentationConvention(DictionaryRepresentation dictionaryRepresentation = DictionaryRepresentation.ArrayOfDocuments)
    {
        _dictionaryRepresentation = dictionaryRepresentation;
    }

    public void Apply(BsonMemberMap memberMap)
    {
        var serializer = memberMap.GetSerializer();
        if (serializer is IDictionaryRepresentationConfigurable dictionaryRepresentationConfigurable)
        {
            var reconfiguredSerializer = dictionaryRepresentationConfigurable.WithDictionaryRepresentation(_dictionaryRepresentation);
            memberMap.SetSerializer(reconfiguredSerializer);
        }
    }
}

public class EmptyContainerSerializeMethodConvention : ConventionBase, IMemberMapConvention
{
    public void Apply(BsonMemberMap memberMap)
    {
        if (memberMap.MemberType.IsGenericType)
        {
            var genType = memberMap.MemberType.GetGenericTypeDefinition();
            if (genType == typeof(List<>))
            {
                memberMap.SetShouldSerializeMethod(o =>
                {
                    var value = memberMap.Getter(o);
                    if (value is IList list)
                    {
                        return list != null && list.Count > 0;
                    }

                    return true;
                });
            }

            else if (genType == typeof(ConcurrentDictionary<,>) || genType == typeof(Dictionary<,>))
            {
                memberMap.SetShouldSerializeMethod(o =>
                {
                    if (o != null)
                    {
                        var value = memberMap.Getter(o);
                        if (value != null)
                        {
                            var countProperty = value.GetType().GetProperty("Count");
                            if (countProperty != null)
                            {
                                var count = (int)countProperty.GetValue(value, null);
                                return count > 0;
                            }
                        }
                    }

                    return true;
                });
            }
        }
    }
}

public static class BsonClassMapHelper
{
    public static void SetConvention()
    {
        ConventionRegistry.Register(nameof(DictionaryRepresentationConvention),
                                    new ConventionPack { new DictionaryRepresentationConvention(), }, _ => true);

        ConventionRegistry.Register(nameof(EmptyContainerSerializeMethodConvention),
                                    new ConventionPack { new EmptyContainerSerializeMethodConvention(), }, _ => true);
    }

    /// <summary>
    /// 提前注册,简化多态类型处理
    /// </summary>
    /// <param name="assembly"></param>
    public static void RegisterAllClass(Assembly assembly)
    {
        var types = assembly.GetTypes();
        foreach (var t in types)
        {
            try
            {
                if (!BsonClassMap.IsClassMapRegistered(t))
                {
                    RegisterClass(t);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error(e);
            }
        }
    }

    public static void RegisterClass(Type t)
    {
        var bsonClassMap = new BsonClassMap(t);
        bsonClassMap.AutoMap();
        bsonClassMap.SetIgnoreExtraElements(true);
        bsonClassMap.SetIgnoreExtraElementsIsInherited(true);
        BsonClassMap.RegisterClassMap(bsonClassMap);
    }
}