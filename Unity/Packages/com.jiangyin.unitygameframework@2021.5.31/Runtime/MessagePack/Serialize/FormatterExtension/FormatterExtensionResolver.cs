
using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Protocol;

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
