using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using URL_Shortener.Data;
using URL_Shortener.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace URL_Shortener.Services
{
    public class CleanupServices : IHostedService, IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<CleanupServices> _logger;
        private Timer _timer;

        public CleanupServices(ApplicationDbContext dbContext, ILogger<CleanupServices> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("URL Cleanup Service started.");
            _timer = new Timer(CleanupExpiredUrls, null, TimeSpan.Zero, TimeSpan.FromHours(12));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("URL Cleanup Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }

        private async void CleanupExpiredUrls(object state)
        {
            try
            {
                var expiredUrls = await _dbContext.Urls
                    .Where(u => u.ExpirationDate < DateTime.UtcNow)
                    .ToListAsync();

                if (expiredUrls.Any())
                {
                    _dbContext.Urls.RemoveRange(expiredUrls);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation($"{expiredUrls.Count} expired URLs were deleted.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error cleaning up expired URLs: {ex.Message}");
            }
        }
    }
}
