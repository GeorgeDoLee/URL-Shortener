using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using URL_Shortener.Data;
using URL_Shortener.Models.Entities;
using URL_Shortener.Services;
using static System.Net.WebRequestMethods;

namespace URL_Shortener.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UrlController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _dbContext;

        public UrlController(IHttpContextAccessor httpContextAccessor, ApplicationDbContext dbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        [HttpPost]
        public async Task<IActionResult> ShortenUrl([FromBody] string originalUrl)
        {
            try
            {
                var url = await UrlServices.AlreadyShortenedAsync(originalUrl, _dbContext);

                if (url == null)
                {
                    Guid id = Guid.NewGuid();
                    string shortCode = UrlServices.GuidToBase62(id);
                    string host = $"{_httpContextAccessor.HttpContext.Request.Scheme}://{_httpContextAccessor.HttpContext.Request.Host}";
                    url = new Url
                    {
                        Id = id,
                        OriginalUrl = originalUrl,
                        ShortenedUrl = $"{host}/{shortCode}",
                        ExpirationDate = DateTime.UtcNow.AddHours(12),
                    };

                    _dbContext.Urls.Add(url);
                    await _dbContext.SaveChangesAsync();
                }

                return Ok(url);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{shortCode}")]
        public async Task<IActionResult> RedirectToOriginal(string shortCode)
        {
            try
            {
                Guid id = UrlServices.Base62ToGuid(shortCode);

                var url = await _dbContext.Urls.FindAsync(id);

                if (url == null)
                {
                    return NotFound("The short URL does not exist.");
                }

                var isExpired = await UrlServices.IsExpiredAsync(id, _dbContext);

                if (isExpired)
                {
                    return Ok("This URL has expired.");
                }

                return Redirect(url.OriginalUrl);
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }

    }
}
