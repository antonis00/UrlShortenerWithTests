using UrlShortener.Models;
using UrlShortener.Shared;

namespace UrlShortener.Interfaces;

public interface IUrlShortenerDataService
{
    Task<Result<bool>> AddUrlAsync(Url url);
    Task<Result<Url?>> GetLongUrlAsync(string shortUrl);
}
