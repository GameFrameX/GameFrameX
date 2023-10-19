using UnityEngine;

public static class UnityEngineVector2Extension
{
    /// <summary>
    /// 取 <see cref="Vector2" /> 的 (x, y) 转换为 <see cref="Vector3" /> 的 (x, 0, y)。
    /// </summary>
    /// <param name="vector2">要转换的 Vector2。</param>
    /// <returns>转换后的 Vector3。</returns>
    public static Vector3 ToVector3(this Vector2 vector2)
    {
        return new Vector3(vector2.x, 0f, vector2.y);
    }

    /// <summary>
    /// 取 <see cref="Vector2" /> 的 (x, y) 和给定参数 y 转换为 <see cref="Vector3" /> 的 (x, 参数 y, y)。
    /// </summary>
    /// <param name="vector2">要转换的 Vector2。</param>
    /// <param name="y">Vector3 的 y 值。</param>
    /// <returns>转换后的 Vector3。</returns>
    public static Vector3 ToVector3(this Vector2 vector2, float y)
    {
        return new Vector3(vector2.x, y, vector2.y);
    }
}