namespace GameFrameX.ProtoExport;

public class ModeAttribute : Attribute
{
    public ModeType Mode { get; }

    public ModeAttribute(ModeType mode)
    {
        Mode = mode;
    }
}