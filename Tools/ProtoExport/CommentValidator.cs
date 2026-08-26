namespace GameFrameX.ProtoExport;

public static class CommentValidator
{
    public static void Validate(MessageInfoList messageInfoList, CommentValidationLevel level)
    {
        var errors = new List<string>();

        foreach (var info in messageInfoList.Infos)
        {
            var kind = info.IsEnum ? "enum" : "message";

            if (level.HasFlag(CommentValidationLevel.Container) && string.IsNullOrWhiteSpace(info.Description))
            {
                errors.Add($"{messageInfoList.FileName}: {kind} '{info.Name}' 缺少注释");
            }

            if (level.HasFlag(CommentValidationLevel.Member))
            {
                foreach (var field in info.Fields)
                {
                    if (!field.IsValid) continue;

                    if (string.IsNullOrWhiteSpace(field.Description))
                    {
                        errors.Add($"{messageInfoList.FileName}: '{info.Name}.{field.Name}' 缺少注释");
                    }
                }
            }
        }

        if (errors.Count > 0)
        {
            throw new Exception("注释校验失败:\n  " + string.Join("\n  ", errors));
        }
    }
}
