using UrlShortener.Models;
using UrlShortener.Shared;

namespace UrlShortener.Interfaces;

public interface IUrlShortenerService
{
    Task<Result<Url>> ShortenUrlAsync(string originalUrl);
    Task<Result<Url>> GetUrlAsync(string shortUrl);
}
