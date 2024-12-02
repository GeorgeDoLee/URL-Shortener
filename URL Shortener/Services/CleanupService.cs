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
    public class CleanupService : IHostedService, IDisposable
    {
        private readonly ILogger<CleanupService> _logger;
        private readonly UrlService _urlService;
        private Timer _timer;

        public CleanupService(UrlService urlService, ILogger<CleanupService> logger)
        {
            _logger = logger;
            _urlService = urlService;
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
                var deletedUrls = await _urlService.RemoveExpiredUrlsAsync();

                _logger.LogInformation($"{deletedUrls} expired URLs were deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error cleaning up expired URLs: {ex.Message}");
            }
        }
    }
}
