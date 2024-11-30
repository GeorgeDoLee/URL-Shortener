using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System.Text;
using URL_Shortener.Data;
using URL_Shortener.Models.Entities;

namespace URL_Shortener.Services
{
    public static class UrlServices
    {
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
    }
}
