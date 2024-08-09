using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using UrlShortener.Data;
using UrlShortener.DTO;
using UrlShortener.Models;

namespace UrlShortener.Tests.IntegrationTests
{
    public class UrlShortenerControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;

        public UrlShortenerControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = ConfigureFactoryWithInMemoryDatabase(factory);
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task ShortenUrl_ShouldReturnCreatedUrl()
        {
            // Arrange
            var originalUrlDto = new OriginalUrlDto { LongUrl = "http://example.com" };
            var content = new StringContent(JsonConvert.SerializeObject(originalUrlDto), Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/UrlShortener/shorten", content);
            response.EnsureSuccessStatusCode();

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("shortUrl", responseContent);
        }

        [Fact]
        public async Task GetUrl_WithValidShortUrl_ShouldReturnOriginalUrl()
        {
            // Arrange
            var originalUrl = $"http://example{DateTime.Now}.com";
            var shortUrl = await SeedDatabaseAsync(originalUrl);

            // Act
            var response = await _client.GetAsync($"/api/UrlShortener/shortUrl?shortUrl={shortUrl}");
            response.EnsureSuccessStatusCode();

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Equal(originalUrl, responseContent);
        }

        [Fact]
        public async Task GetUrl_WithInvalidShortUrl_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/UrlShortener/nonexistent");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task ShortenUrl_ShouldReturnInternalServerError_OnDbFailure()
        {
            // Arrange
            var failingFactory = CreateFactoryWithFailingMockDB();
            var clientWithMockDbFailure = failingFactory.CreateClient();
            var originalUrlDto = new OriginalUrlDto { LongUrl = "http://example.com" };
            var content = new StringContent(JsonConvert.SerializeObject(originalUrlDto), Encoding.UTF8, "application/json");

            // Act
            var response = await clientWithMockDbFailure.PostAsync("/api/UrlShortener/shorten", content);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        }

        private WebApplicationFactory<Program> ConfigureFactoryWithInMemoryDatabase(WebApplicationFactory<Program> factory)
        {
            return factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext registration if any
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<UrlContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Register an in-memory database for testing
                    services.AddDbContext<UrlContext>(options =>
                    {
                        options.UseInMemoryDatabase("UrlShortenerIntegrationTestDb");
                    });
                });
            });
        }

        private WebApplicationFactory<Program> CreateFactoryWithFailingMockDB()
        {
            return _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<UrlContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add an in-memory database for testing
                    services.AddDbContext<UrlContext>(options =>
                    {
                        options.UseInMemoryDatabase("UrlShortenerIntegrationTestDbWithFailure");
                    });

                    // Mock DbContext to simulate failure
                    services.AddScoped<UrlContext>(provider =>
                    {
                        var options = provider.GetRequiredService<DbContextOptions<UrlContext>>();
                        var mockContext = new Mock<UrlContext>(options);
                        mockContext.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                            .ThrowsAsync(new DbUpdateException("Database failure"));
                        return mockContext.Object;
                    });
                });
            });
        }

        private async Task<string> SeedDatabaseAsync(string originalUrl)
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<UrlContext>();
                var url = new Url { OriginalUrl = originalUrl, ShortUrl = DateTime.Now.ToString() };
                context.Urls.Add(url);
                await context.SaveChangesAsync();
                return url.ShortUrl;
            }
        }
    }
}
