using Microsoft.EntityFrameworkCore;
using UrlShortener.Data;
using UrlShortener.DataServices;
using UrlShortener.Models;

namespace UrlShortener.Tests.UnitTests.DataServices
{
    public class UrlShortenerDataServiceTests
    {
        private readonly UrlContext _context;
        private readonly UrlShortenerDataService _dataService;

        public UrlShortenerDataServiceTests()
        {
            var options = new DbContextOptionsBuilder<UrlContext>()
                .UseInMemoryDatabase(databaseName: "UrlShortenerTestDb")
                .Options;
            _context = new UrlContext(options);
            _dataService = new UrlShortenerDataService(_context);
        }

        #region AddUrlAsync

        [Fact]
        public async Task AddUrlAsync_ShouldAddUrl()
        {
            // Arrange
            var url = new Url { OriginalUrl = "http://example.com", ShortUrl = "abc123" };

            // Act
            await _dataService.AddUrlAsync(url);
            var savedUrl = await _context.Urls.FirstOrDefaultAsync(u => u.ShortUrl == "abc123");

            // Assert
            Assert.NotNull(savedUrl);
            Assert.Equal("http://example.com", savedUrl.OriginalUrl);
        }

        #endregion

        #region GetLongUrlAsync

        [Fact]
        public async Task GetLongUrlAsync_WithExistingShortUrl_ShouldReturnUrl()
        {
            // Arrange
            var url = new Url { OriginalUrl = "http://example.com", ShortUrl = "abc123" };
            _context.Urls.Add(url);
            await _context.SaveChangesAsync();

            // Act
            var result = await _dataService.GetLongUrlAsync("abc123");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("http://example.com", result?.Value?.OriginalUrl);
        }

        [Fact]
        public async Task GetLongUrlAsync_WithNonExistingShortUrl_ShouldReturnNull()
        {
            // Act
            var result = await _dataService.GetLongUrlAsync("nonexistent");

            // Assert
            Assert.Null(result.Value);
        }

        #endregion
    }
}
