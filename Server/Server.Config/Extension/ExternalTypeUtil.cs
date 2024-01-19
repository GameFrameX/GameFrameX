using System.Numerics;

public static class ExternalTypeUtil
{
    public static Vector2 NewVector2(cfg.vec2 v)
    {
        return new Vector2(v.X, v.Y);
    }

    public static Vector3 NewVector3(cfg.vec3 v)
    {
        return new Vector3(v.X, v.Y, v.Z);
    }

    public static Vector4 NewVector4(cfg.vec4 v)
    {
        return new Vector4(v.X, v.Y, v.Z, v.W);
    }
}