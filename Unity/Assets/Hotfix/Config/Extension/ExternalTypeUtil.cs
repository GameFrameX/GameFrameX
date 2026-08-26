using Hotfix.Config;

public static class ExternalTypeUtil
{
    public static UnityEngine.Vector2 NewVector2(vec2 v)
    {
        return new UnityEngine.Vector2(v.X, v.Y);
    }

    public static UnityEngine.Vector3 NewVector3(vec3 v)
    {
        return new UnityEngine.Vector3(v.X, v.Y, v.Z);
    }

    public static UnityEngine.Vector4 NewVector4(vec4 v)
    {
        return new UnityEngine.Vector4(v.X, v.Y, v.Z, v.W);
    }
}