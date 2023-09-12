using MessagePack;
using MessagePack.Formatters;
using System;

namespace FormatterExtension
{
    public class FormatterExtensionResolver : IFormatterResolver
    {
        public static readonly FormatterExtensionResolver Instance = new FormatterExtensionResolver();

        private FormatterExtensionResolver()
        {
        }

        // public Func<T, IMessagePackFormatter<T>> FindFormatter<T>;
        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            if (typeof(T) == typeof(DateTime))
            {
                return LocalDateTimeFormatter.Instance as IMessagePackFormatter<T>;
            }

            return null;
            // return FindFormatter?.Invoke(T);
        }
    }
}