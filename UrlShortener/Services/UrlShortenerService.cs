using UrlShortener.Interfaces;
using UrlShortener.Models;
using UrlShortener.Shared;

namespace UrlShortener.Services;

public class UrlShortenerService : IUrlShortenerService
{
    private readonly IUrlShortenerDataService _urlShortenerDataService;

    public UrlShortenerService(IUrlShortenerDataService urlShortenerDataService)
    {
        _urlShortenerDataService = urlShortenerDataService;
    }

    public async Task<Result<Url>> GetUrlAsync(string shortUrl)
    {
        Result<Url?> result = await _urlShortenerDataService.GetLongUrlAsync(UrlHelper.Strip(shortUrl));

        return result.PropagateResult();
    }

    public async Task<Result<Url>> ShortenUrlAsync(string longUrl)
    {
        if (IsInvalid(longUrl)) { return Result<Url>.Failure("Invalid URL!"); }

        Url shortenedUrl = ConversionHelper.EncodeUrl(longUrl);

        Result<bool> result = await _urlShortenerDataService.AddUrlAsync(shortenedUrl);

        return result.PropagateResult(shortenedUrl);
    }

    #region private methods

    private bool IsInvalid(string url)
    {
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult) &&
            (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps))
        {
            return false;
        }
        return true;
    }

    #endregion

}
