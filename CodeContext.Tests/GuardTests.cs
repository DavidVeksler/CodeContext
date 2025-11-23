using CodeContext.Utils;
using Xunit;

namespace CodeContext.Tests;

/// <summary>
/// Tests for the Guard utility class that provides parameter validation.
/// </summary>
public class GuardTests
{
    [Fact]
    public void NotNull_WithValidReference_ReturnsValue()
    {
        // Arrange
        var testObject = new object();

        // Act
        var result = Guard.NotNull(testObject, nameof(testObject));

        // Assert
        Assert.Same(testObject, result);
    }

    [Fact]
    public void NotNull_WithNullReference_ThrowsArgumentNullException()
    {
        // Arrange
        object? testObject = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            Guard.NotNull(testObject, "testObject"));
        Assert.Equal("testObject", exception.ParamName);
    }

    [Fact]
    public void NotNullOrEmpty_WithValidString_ReturnsValue()
    {
        // Arrange
        var testString = "valid string";

        // Act
        var result = Guard.NotNullOrEmpty(testString, nameof(testString));

        // Assert
        Assert.Equal(testString, result);
    }

    [Fact]
    public void NotNullOrEmpty_WithNullString_ThrowsArgumentException()
    {
        // Arrange
        string? testString = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Guard.NotNullOrEmpty(testString, "testString"));
        Assert.Equal("testString", exception.ParamName);
    }

    [Fact]
    public void NotNullOrEmpty_WithEmptyString_ThrowsArgumentException()
    {
        // Arrange
        var testString = string.Empty;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            Guard.NotNullOrEmpty(testString, "testString"));
        Assert.Equal("testString", exception.ParamName);
    }

    [Fact]
    public void DirectoryExists_WithExistingDirectory_ReturnsPath()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var result = Guard.DirectoryExists(tempDir, nameof(tempDir));

            // Assert
            Assert.Equal(tempDir, result);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir);
        }
    }

    [Fact]
    public void DirectoryExists_WithNonExistentDirectory_ThrowsDirectoryNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act & Assert
        Assert.Throws<DirectoryNotFoundException>(() =>
            Guard.DirectoryExists(nonExistentPath, "testPath"));
    }

    [Fact]
    public void DirectoryExists_WithNullPath_ThrowsArgumentException()
    {
        // Arrange
        string? nullPath = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Guard.DirectoryExists(nullPath!, "testPath"));
    }

    [Fact]
    public void FileExists_WithExistingFile_ReturnsPath()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();

        try
        {
            // Act
            var result = Guard.FileExists(tempFile, nameof(tempFile));

            // Assert
            Assert.Equal(tempFile, result);
        }
        finally
        {
            // Cleanup
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void FileExists_WithNonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");

        // Act & Assert
        Assert.Throws<FileNotFoundException>(() =>
            Guard.FileExists(nonExistentPath, "testPath"));
    }

    [Fact]
    public void FileExists_WithNullPath_ThrowsArgumentException()
    {
        // Arrange
        string? nullPath = null;

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Guard.FileExists(nullPath!, "testPath"));
    }
}
