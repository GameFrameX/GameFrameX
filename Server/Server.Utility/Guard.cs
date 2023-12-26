namespace Server.Utility;

public static class Guard
{
    /// <summary>
    /// 确保指定的值不为null。
    /// </summary>
    /// <param name="value">要检查的值。</param>
    /// <param name="name">值的名称。</param>
    /// <exception cref="ArgumentNullException">当值为null时引发。</exception>
    public static void NotNullOrEmpty(string value, string name)
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentNullException(name, " can not be null.");
        }
    }

    /// <summary>
    /// 确保指定的值不为null。
    /// </summary>
    /// <typeparam name="T">值的类型。</typeparam>
    /// <param name="value">要检查的值。</param>
    /// <param name="name">值的名称。</param>
    /// <exception cref="ArgumentNullException">当值为null时引发。</exception>
    public static void NotNull<T>(T value, string name) where T : class
    {
        if (value == null)
        {
            throw new ArgumentNullException(name, " can not be null.");
        }
    }
}