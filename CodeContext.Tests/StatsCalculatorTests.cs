using CodeContext.Services;
using Xunit;

namespace CodeContext.Tests;

/// <summary>
/// Tests for the StatsCalculator class that calculates and formats project statistics.
/// </summary>
public class StatsCalculatorTests : IDisposable
{
    private readonly StatsCalculator _calculator;
    private readonly string _tempRoot;

    public StatsCalculatorTests()
    {
        _calculator = new StatsCalculator();
        _tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempRoot);
    }

    [Fact]
    public void Calculate_WithValidInputs_ReturnsFormattedStats()
    {
        // Arrange
        var testFile = Path.Combine(_tempRoot, "test.txt");
        File.WriteAllText(testFile, "line1\nline2\nline3");

        var content = "test content\nwith multiple\nlines\n";
        var elapsed = TimeSpan.FromSeconds(1.5);

        try
        {
            // Act
            var result = _calculator.Calculate(_tempRoot, content, elapsed);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("Stats:", result);
            Assert.Contains("Files processed:", result);
            Assert.Contains("Total lines:", result);
            Assert.Contains("Time taken:", result);
            Assert.Contains("Output size:", result);
        }
        finally
        {
            File.Delete(testFile);
        }
    }

    [Fact]
    public void Calculate_CountsLinesCorrectly()
    {
        // Arrange
        var content = "line1\nline2\nline3\n";
        var elapsed = TimeSpan.FromSeconds(1);

        // Act
        var result = _calculator.Calculate(_tempRoot, content, elapsed);

        // Assert
        Assert.Contains("Total lines: 3", result);
    }

    [Fact]
    public void Calculate_WithEmptyContent_CountsZeroLines()
    {
        // Arrange
        var content = "";
        var elapsed = TimeSpan.FromSeconds(1);

        // Act
        var result = _calculator.Calculate(_tempRoot, content, elapsed);

        // Assert
        Assert.Contains("Total lines: 0", result);
    }

    [Fact]
    public void Calculate_CountsFilesCorrectly()
    {
        // Arrange
        var file1 = Path.Combine(_tempRoot, "file1.txt");
        var file2 = Path.Combine(_tempRoot, "file2.txt");
        var subDir = Path.Combine(_tempRoot, "subdir");
        Directory.CreateDirectory(subDir);
        var file3 = Path.Combine(subDir, "file3.txt");

        File.WriteAllText(file1, "test");
        File.WriteAllText(file2, "test");
        File.WriteAllText(file3, "test");

        var content = "test content";
        var elapsed = TimeSpan.FromSeconds(1);

        try
        {
            // Act
            var result = _calculator.Calculate(_tempRoot, content, elapsed);

            // Assert
            Assert.Contains("Files processed: 3", result);
        }
        finally
        {
            File.Delete(file1);
            File.Delete(file2);
            File.Delete(file3);
            Directory.Delete(subDir);
        }
    }

    [Fact]
    public void Calculate_FormatsTimeCorrectly()
    {
        // Arrange
        var content = "test";
        var elapsed = TimeSpan.FromSeconds(2.456);

        // Act
        var result = _calculator.Calculate(_tempRoot, content, elapsed);

        // Assert
        Assert.Contains("Time taken: 2.46s", result);
    }

    [Fact]
    public void Calculate_IncludesOutputSize()
    {
        // Arrange
        var content = "12345"; // 5 characters
        var elapsed = TimeSpan.FromSeconds(1);

        // Act
        var result = _calculator.Calculate(_tempRoot, content, elapsed);

        // Assert
        Assert.Contains("Output size: 5 characters", result);
    }

    [Fact]
    public void Calculate_WithNonExistentDirectory_HandlesGracefully()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var content = "test content";
        var elapsed = TimeSpan.FromSeconds(1);

        // Act
        var result = _calculator.Calculate(nonExistentPath, content, elapsed);

        // Assert
        Assert.NotNull(result);
        // Should still return a result, possibly with error message
        Assert.Contains("Stats:", result);
    }

    [Fact]
    public void Calculate_WithMultipleNewlines_CountsCorrectly()
    {
        // Arrange
        var content = "line1\n\n\nline2\n";
        var elapsed = TimeSpan.FromSeconds(1);

        // Act
        var result = _calculator.Calculate(_tempRoot, content, elapsed);

        // Assert
        Assert.Contains("Total lines: 4", result);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempRoot))
            {
                Directory.Delete(_tempRoot, true);
            }
        }
        catch
        {
            // Best effort cleanup
        }
    }
}
