using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.Interfaces;
using UrlShortener.Models;
using UrlShortener.Shared;

namespace UrlShortener.DataServices;

public class UrlShortenerDataService : IUrlShortenerDataService
{
    private readonly UrlContext _context;

    public UrlShortenerDataService(UrlContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> AddUrlAsync(Url url)
    {
        try
        {
            _context.Urls.Add(url);
            await _context.SaveChangesAsync();
            return Result<bool>.Success(true);
        }
        catch (Exception)
        {
            return Result<bool>.Failure("An error occurred while adding the URL to the database");
        }

    }

    public async Task<Result<Url?>> GetLongUrlAsync(string shortUrl)
    {
        try
        {
            Url? longUrl = await _context.Urls.AsNoTracking().FirstOrDefaultAsync(u => u.ShortUrl == shortUrl);

            if (longUrl is null) { return Result<Url?>.Failure("Short URL not found!"); }

            return Result<Url?>.Success(longUrl);
        }
        catch (Exception)
        {
            return Result<Url?>.Failure("An error occurred while fetching the URL from the database");
        }
    }
}
