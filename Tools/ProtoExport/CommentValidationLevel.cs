namespace GameFrameX.ProtoExport;

[Flags]
public enum CommentValidationLevel
{
    None = 0,
    Container = 1,
    Member = 2,
    All = Container | Member
}
