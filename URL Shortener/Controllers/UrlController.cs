using Microsoft.AspNetCore.Mvc;
using URL_Shortener.Services;
using static System.Net.WebRequestMethods;

namespace URL_Shortener.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UrlController : ControllerBase
    {
        private readonly UrlService _urlService;

        public UrlController(UrlService urlService)
        {
            _urlService = urlService;
        }


        [HttpPost]
        public async Task<IActionResult> ShortenUrl([FromBody] string originalUrl)
        {
            var url = await _urlService.ShortenUrlAsync(originalUrl);

            return Ok(url);
        }


        [HttpGet("{shortCode}")]
        public async Task<IActionResult> RedirectToOriginal(string shortCode)
        {
            var originalUrl = await _urlService.GetOriginalUrlAsync(shortCode);

            return Redirect(originalUrl);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveUrl(int id)
        {
            var urlRemoved = await _urlService.RemoveUrlAsync(id);

            if(!urlRemoved)
            {
                return NotFound("Url Not Found.");
            }

            return Ok("Url Removed Successfully.");
        }
    }
}
