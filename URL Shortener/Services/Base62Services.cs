using Base62;
using System;

namespace URL_Shortener.Services
{
    public static class Base62Services
    {
        private static readonly Base62Converter _base62Converter = new Base62Converter();

        public static string GuidToBase62(Guid guid)
        {
            return _base62Converter.Encode(guid.ToString());
        }

        public static Guid Base62ToGuid(string base62)
        {
            string decodedString = _base62Converter.Decode(base62);

            return new Guid(decodedString);
        }
    }
}
