namespace GameFrameX.ProtoExport;

public enum ModeType
{
    /// <summary>
    /// C# 语言（Server, Unity, Godot, Stride, Flax 等）
    /// </summary>
    CSharp,

    /// <summary>
    /// TypeScript 语言（LayaAir, Cocos Creator, Phaser 等）
    /// </summary>
    TypeScript,

    /// <summary>
    /// C++ 语言（Unreal Engine 等）
    /// </summary>
    Cpp,

    /// <summary>
    /// Lua 语言（Defold, Solar2D, Dora SSR 等）
    /// </summary>
    Lua,

    /// <summary>
    /// Go 语言（Go 游戏服务器等）
    /// </summary>
    Go
}
