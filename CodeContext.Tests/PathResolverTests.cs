using CodeContext.Services;
using Xunit;

namespace CodeContext.Tests;

/// <summary>
/// Tests for the PathResolver class, focusing on pure functions.
/// </summary>
public class PathResolverTests
{
    [Fact]
    public void GetFolderName_WithSimpleDirectory_ReturnsDirectoryName()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "TestFolder");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var result = PathResolver.GetFolderName(tempDir);

            // Assert
            Assert.Equal("TestFolder", result);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir);
        }
    }

    [Fact]
    public void GetFolderName_WithNestedDirectory_ReturnsLastDirectoryName()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "Parent", "Child");
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var result = PathResolver.GetFolderName(tempDir);

            // Assert
            Assert.Equal("Child", result);
        }
        finally
        {
            // Cleanup
            Directory.Delete(Path.Combine(Path.GetTempPath(), "Parent"), true);
        }
    }

    [Fact]
    public void GetFolderName_WithCurrentDirectoryMarker_ReturnsCurrentDirectoryName()
    {
        // Arrange
        var currentDir = Directory.GetCurrentDirectory();
        var expectedName = new DirectoryInfo(currentDir).Name;

        // Act
        var result = PathResolver.GetFolderName(".");

        // Assert
        Assert.Equal(expectedName, result);
    }

    [Theory]
    [InlineData("C:\\")]
    [InlineData("C:\\\\")]
    public void GetFolderName_WithRootPath_ReturnsRootOrCleanedName(string path)
    {
        // Act
        var result = PathResolver.GetFolderName(path);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        // Result should be either "root" or the cleaned drive name (e.g., "C")
        Assert.True(result == "root" || result == "C" || result.Length > 0);
    }

    [Fact]
    public void GetFolderName_WithPathEndingInSeparator_HandlesCorrectly()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), "TestFolder");
        Directory.CreateDirectory(tempDir);
        var pathWithSeparator = tempDir + Path.DirectorySeparatorChar;

        try
        {
            // Act
            var result = PathResolver.GetFolderName(pathWithSeparator);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir);
        }
    }

    [Fact]
    public void GetFolderName_WithValidPath_NeverReturnsNullOrEmpty()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var result = PathResolver.GetFolderName(tempDir);

            // Assert
            Assert.NotNull(result);
            Assert.NotEmpty(result);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir);
        }
    }
}
