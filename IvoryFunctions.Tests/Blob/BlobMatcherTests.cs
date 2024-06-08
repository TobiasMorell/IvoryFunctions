using IvoryFunctions.Blob.Implementations;

namespace Functions.MassTransit.Tests.Blob;

public class BlobMatcherTests
{
    [Theory]
    [InlineData("test.txt", "test.txt", true)]
    [InlineData("test.txt", "test.pdf", false)]
    [InlineData("test.txt", "cake.txt", false)]
    [InlineData("path/to/test.txt", "path/to/test.txt", true)]
    [InlineData("path/to/{file}", "path/to/test.txt", true)]
    [InlineData("path/to/{file}.pdf", "path/to/test.txt", false)]
    [InlineData("path-to/{file}", "path/to/file.txt", false)]
    [InlineData("path-to/{file}-trigger.txt", "path-to/test-trigger.txt", true)]
    [InlineData("path-to/{file}-trigger.txt", "path-to/test-trigger.pdf", false)]
    [InlineData("path-to/{file}-trigger.txt", "path-to/test-trigger", false)]
    public void IsMatchingPath(string pattern, string path, bool shouldMatch)
    {
        // Act
        var result = new BlobFileMatcher().IsMatchingPath(path, pattern);

        // Assert
        result.ShouldBe(shouldMatch);
    }
}
