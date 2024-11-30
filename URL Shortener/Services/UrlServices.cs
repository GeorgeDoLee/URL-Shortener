using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System.Text;
using URL_Shortener.Data;
using URL_Shortener.Models.Entities;

namespace URL_Shortener.Services
{
    public static class UrlServices
    {
        private static readonly string Base62Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        public static async Task<Url> AlreadyShortenedAsync(string originalUrl, ApplicationDbContext dbContext)
        {
            var url = await dbContext.Urls
                .Where(u => u.OriginalUrl == originalUrl)
                .FirstOrDefaultAsync();

            return url;
        }

        public static async Task<bool> IsExpiredAsync(Guid id, ApplicationDbContext dbContext)
        {
            var url = await dbContext.Urls.FindAsync(id);

            if (url == null)
            {
                return false;
            }

            if (DateTime.UtcNow > url.ExpirationDate)
            {
                dbContext.Urls.Remove(url);
                await dbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }

        public static string GuidToBase62(Guid id)
        {
            byte[] bytes = id.ToByteArray();

            BigInteger bigInt = new BigInteger(bytes);

            if (bigInt < 0)
            {
                bigInt = -bigInt;
            }

            StringBuilder base62 = new StringBuilder();
            while (bigInt > 0)
            {
                int remainder = (int)(bigInt % 62);
                base62.Insert(0, Base62Chars[remainder]);
                bigInt /= 62;
            }

            return base62.ToString();
        }

        public static Guid Base62ToGuid(string shortCode)
        {
            BigInteger bigInt = BigInteger.Zero;

            foreach (char c in shortCode)
            {
                int value = Base62Chars.IndexOf(c);
                if (value == -1)
                {
                    throw new ArgumentException("Invalid character in Base62 string", nameof(shortCode));
                }

                bigInt = bigInt * 62 + value;
            }

            byte[] bytes = bigInt.ToByteArray();

            if (bytes.Length < 16)
            {
                byte[] paddedBytes = new byte[16];
                Array.Copy(bytes, 0, paddedBytes, 16 - bytes.Length, bytes.Length);
                bytes = paddedBytes;
            }

            return new Guid(bytes);
        }
    }
}
