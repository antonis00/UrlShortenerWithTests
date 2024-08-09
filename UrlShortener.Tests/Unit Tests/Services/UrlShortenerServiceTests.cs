using Moq;
using UrlShortener.Models;
using UrlShortener.Services;
using UrlShortener.Interfaces;

namespace UrlShortener.Tests.UnitTests.Services;

public class UrlShortenerServiceTests
{
    private readonly UrlShortenerService _urlShortenerService;
    private readonly Mock<IUrlShortenerDataService> _urlShortenerDataServiceMock;

    public UrlShortenerServiceTests()
    {
        _urlShortenerDataServiceMock = new Mock<IUrlShortenerDataService>();
        _urlShortenerService = new UrlShortenerService(_urlShortenerDataServiceMock.Object);
    }

    #region ShortenUrlAsync

    [Fact]
    public async Task ShortenUrlAsync_WithValidUrl_ShouldReturnShortenedUrl()
    {
        // Arrange
        var longUrl = "http://example.com";
        _urlShortenerDataServiceMock
            .Setup(x => x.AddUrlAsync(It.IsAny<Url>()))
            .ReturnsAsync(Shared.Result<bool>.Success(true));

        // Act
        var result = await _urlShortenerService.ShortenUrlAsync(longUrl);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(longUrl, result.Value.OriginalUrl);
        Assert.False(string.IsNullOrEmpty(result.Value.ShortUrl));

        _urlShortenerDataServiceMock.Verify(x => x.AddUrlAsync(It.Is<Url>(u => u.OriginalUrl == longUrl)), Times.Once);

    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-url")]
    public async Task ShortenUrlAsync_WithInvalidUrl_ShouldReturnInvalidUrlError(string longUrl)
    {
        // Arrange
        string errorMessage = "Invalid URL!";

        _urlShortenerDataServiceMock
            .Setup(x => x.AddUrlAsync(It.IsAny<Url>()))
            .ReturnsAsync(Shared.Result<bool>.Failure(errorMessage));

        // Act
        var result = await _urlShortenerService.ShortenUrlAsync(longUrl);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(result.Error, errorMessage);

        _urlShortenerDataServiceMock.Verify(x => x.AddUrlAsync(It.IsAny<Url>()), Times.Never);

    }

    #endregion

    #region GetUrlAsync

    [Fact]
    public async Task GetUrlAsync_WithExistingShortUrl_ShouldReturnOriginalUrl()
    {
        // Arrange
        string shortUrl = "short123";
        var url = new Url { OriginalUrl = "http://example.com", ShortUrl = shortUrl };
        _urlShortenerDataServiceMock
            .Setup(x => x.GetLongUrlAsync(It.IsAny<string>()))
            .ReturnsAsync(Shared.Result<Url?>.Success(url));

        // Act
        var result = await _urlShortenerService.GetUrlAsync(shortUrl);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(url.OriginalUrl, result.Value.OriginalUrl);

        _urlShortenerDataServiceMock.Verify(x => x.GetLongUrlAsync(shortUrl), Times.Once);
    }

    [Fact]
    public async Task GetUrlAsync_WithNonExistingShortUrl_ShouldReturnNotFoundError()
    {
        // Arrange
        string shortUrl = "short123";
        string errorMessage = "Short URL not found!";
        _urlShortenerDataServiceMock
            .Setup(x => x.GetLongUrlAsync(It.IsAny<string>()))
            .ReturnsAsync(Shared.Result<Url?>.Failure(errorMessage));

        // Act
        var result = await _urlShortenerService.GetUrlAsync(shortUrl);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(errorMessage, result.Error);

        _urlShortenerDataServiceMock.Verify(x => x.GetLongUrlAsync(shortUrl), Times.Once);
    }

    #endregion
}
