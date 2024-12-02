using Base62;
using Microsoft.EntityFrameworkCore;
using System;
using URL_Shortener.Data;
using URL_Shortener.Models.Entities;

namespace URL_Shortener.Services
{
    public class UrlService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly string _host;
        private static readonly Base62Converter _base62Converter = new Base62Converter();

        public UrlService(IConfiguration configuration, ApplicationDbContext dbContext)
        {
            _host = $"{configuration["Server:Host"]}:{configuration["Server:Port"]}/api/Url";
            _dbContext = dbContext;
        }


        public async Task<Url> AlreadyShortenedAsync(string originalUrl)
        {
            var url = await _dbContext.Urls
                .Where(u => u.OriginalUrl == originalUrl)
                .FirstOrDefaultAsync();

            return url;
        }
        

        public async Task<bool> IsExpiredAsync(int id)
        {
            var url = await _dbContext.Urls.FindAsync(id);

            if (url == null)
            {
                return false;
            }

            if (DateTime.UtcNow > url.ExpirationDate)
            {
                _dbContext.Urls.Remove(url);
                await _dbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }


        public string Encode(int id)
        {
            string encodedId = _base62Converter.Encode(id.ToString());
            return encodedId;
        }


        public int Decode(string shortCode)
        {
            string decodedId = _base62Converter.Decode(shortCode);

            return int.Parse(decodedId);
        }


        public async Task<Url> ShortenUrlAsync(string originalUrl)
        {
            var url = await AlreadyShortenedAsync(originalUrl);

            if (url == null)
            {
                url = new Url
                {
                    OriginalUrl = originalUrl,
                    ShortenedUrl = "",
                    ExpirationDate = DateTime.UtcNow.AddHours(12),
                };

                _dbContext.Urls.Add(url);
                await _dbContext.SaveChangesAsync();

                string shortCode = Encode(url.Id);

                url.ShortenedUrl = $"{_host}/{shortCode}";
                await _dbContext.SaveChangesAsync();
            }

            return url;
        }


        public async Task<string> GetOriginalUrlAsync(string shortCode)
        {
            int id = Decode(shortCode);

            var url = await _dbContext.Urls.FindAsync(id);

            if(url == null)
            {
                throw new Exception("Url Not Found.");
            }

            var isExpired = await IsExpiredAsync(id);

            if (isExpired)
            {
                throw new Exception("Url Is Expired.");
            }

            return url.OriginalUrl;
        }


        public async Task<int> RemoveExpiredUrlsAsync()
        {
            var expiredUrls = await _dbContext.Urls
                    .Where(u => u.ExpirationDate < DateTime.UtcNow)
                    .ToListAsync();


            if (expiredUrls.Any())
            {
                _dbContext.Urls.RemoveRange(expiredUrls);
                await _dbContext.SaveChangesAsync();
                return expiredUrls.Count;
            }

            return 0;
        }


        public async Task<bool> RemoveUrlAsync(int id)
        {
            var url = await _dbContext.Urls.FindAsync(id);

            if (url != null)
            {
                _dbContext.Urls.Remove(url);
                await _dbContext.SaveChangesAsync();

                return true;
            }

            return false;
        }
    }
}
