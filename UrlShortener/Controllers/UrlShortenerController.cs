using Microsoft.AspNetCore.Mvc;
using UrlShortener.Models;
using UrlShortener.Interfaces;
using UrlShortener.DTO;
using UrlShortener.Shared;

namespace UrlShortener.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UrlShortenerController : ControllerBase
{
    private readonly IUrlShortenerService _urlShortenerService;

    public UrlShortenerController(IUrlShortenerService urlShortenerService)
    {
        _urlShortenerService = urlShortenerService;
    }

    [HttpPost("shorten")]
    public async Task<ActionResult<Url>> ShortenUrl([FromBody] OriginalUrlDto urlDto)
    {
        var result = await _urlShortenerService.ShortenUrlAsync(urlDto.LongUrl);

        return result.ToActionResult(url => CreatedAtAction(nameof(GetUrl), new { shortUrl = url.ShortUrl }, new { shortUrl = UrlHelper.Wrap(url.ShortUrl) }));
    }

    [HttpGet("shortUrl")]
    public async Task<IActionResult> GetUrl(string shortUrl)
    {
        var result = await _urlShortenerService.GetUrlAsync(shortUrl);

        return result.ToActionResult(url => Ok(url.OriginalUrl));
    }

}
