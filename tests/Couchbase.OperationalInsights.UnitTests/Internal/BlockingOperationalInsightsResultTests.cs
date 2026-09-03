using Couchbase.OperationalInsightsClient.Internal.Results;
using Couchbase.Core.Json;
using Xunit;

namespace Couchbase.OperationalInsightsClient.UnitTests.Internal;

public class BlockingOperationalInsightsResultTest
{
    [Fact]
    public async Task InitializeAsync_ShouldReadResponseStream()
    {
        // Arrange
        var json = File.ReadAllBytes("JsonDocuments/insightsResponse.json");
        var stream = new MemoryStream(json);
        var result = new BlockingOperationalInsightsResult(stream, new StjJsonDeserializer());

        // Act
        await result.InitializeAsync();

        // Assert
        // No exception should be thrown, and the method should complete successfully.
    }

    [Fact]
    public async Task Error_23000_ShouldHaveErrorMessageAndCode()
    {
        // Arrange
        var json = File.ReadAllBytes("JsonDocuments/error-23000-response.json");
        var stream = new MemoryStream(json);
        var result = new BlockingOperationalInsightsResult(stream, new StjJsonDeserializer());

        // Act
        await result.InitializeAsync();

        // Assert
        Assert.Single(result.Errors);
        var error = result.Errors[0];
        Assert.Equal("Enterprise OperationalInsights is temporarily unavailable", error.Message);
        Assert.Equal(23000, error.Code);
        Assert.True(error.Retriable);
    }

    [Fact]
    public async Task GetAsyncEnumerator_ShouldThrowNotImplementedException()
    {
        // Arrange
        var stream = new MemoryStream();
        var result = new BlockingOperationalInsightsResult(stream, new StjJsonDeserializer());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await result.GetAsyncEnumerator().MoveNextAsync().ConfigureAwait(false));
    }

    [Fact]
    public void Constructor_ShouldInitializeProperties()
    {
        // Arrange
        var stream = new MemoryStream();

        // Act
        var result = new BlockingOperationalInsightsResult(stream, new StjJsonDeserializer());

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task EnumerateRows_SecondEnumeration_ThrowsInvalidOperationException()
    {
        // Arrange
        var json = File.ReadAllBytes("JsonDocuments/insightsResponse.json");
        var stream = new MemoryStream(json);
        var result = new BlockingOperationalInsightsResult(stream, new StjJsonDeserializer());
        await result.InitializeAsync(CancellationToken.None);

        // Act — first enumeration should succeed
        var rows = await result.ToListAsync(CancellationToken.None);
        Assert.NotEmpty(rows);

        // Assert — second enumeration should throw
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await foreach (var _ in result) { }
        });
    }
}
