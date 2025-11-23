using CodeContext.Utils;
using Xunit;

namespace CodeContext.Tests;

/// <summary>
/// Tests for the GitHelper utility class that provides Git repository operations.
/// </summary>
public class GitHelperTests
{
    [Fact]
    public void FindRepositoryRoot_WithNullPath_ReturnsNull()
    {
        // Act
        var result = GitHelper.FindRepositoryRoot(null);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindRepositoryRoot_WithEmptyPath_ReturnsNull()
    {
        // Act
        var result = GitHelper.FindRepositoryRoot(string.Empty);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindRepositoryRoot_WithNonExistentPath_ReturnsNull()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        // Act
        var result = GitHelper.FindRepositoryRoot(nonExistentPath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void FindRepositoryRoot_WithGitRepository_ReturnsRootPath()
    {
        // Arrange - Create a temporary git repository
        var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var gitDir = Path.Combine(tempRoot, ".git");
        var subDir = Path.Combine(tempRoot, "src", "nested");

        Directory.CreateDirectory(gitDir);
        Directory.CreateDirectory(subDir);

        try
        {
            // Act
            var result = GitHelper.FindRepositoryRoot(subDir);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tempRoot, result);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempRoot, true);
        }
    }

    [Fact]
    public void FindRepositoryRoot_WithDirectGitDirectory_ReturnsRootPath()
    {
        // Arrange - Create a temporary git repository
        var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var gitDir = Path.Combine(tempRoot, ".git");

        Directory.CreateDirectory(gitDir);

        try
        {
            // Act
            var result = GitHelper.FindRepositoryRoot(tempRoot);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(tempRoot, result);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempRoot, true);
        }
    }

    [Fact]
    public void FindRepositoryRoot_WithNoGitDirectory_ReturnsNull()
    {
        // Arrange - Create a temporary directory without .git
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var result = GitHelper.FindRepositoryRoot(tempDir);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir);
        }
    }

    [Fact]
    public void IsInRepository_WithNullPath_ReturnsFalse()
    {
        // Act
        var result = GitHelper.IsInRepository(null);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsInRepository_WithGitRepository_ReturnsTrue()
    {
        // Arrange - Create a temporary git repository
        var tempRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var gitDir = Path.Combine(tempRoot, ".git");
        var subDir = Path.Combine(tempRoot, "src");

        Directory.CreateDirectory(gitDir);
        Directory.CreateDirectory(subDir);

        try
        {
            // Act
            var result = GitHelper.IsInRepository(subDir);

            // Assert
            Assert.True(result);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempRoot, true);
        }
    }

    [Fact]
    public void IsInRepository_WithNoGitRepository_ReturnsFalse()
    {
        // Arrange - Create a temporary directory without .git
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Act
            var result = GitHelper.IsInRepository(tempDir);

            // Assert
            Assert.False(result);
        }
        finally
        {
            // Cleanup
            Directory.Delete(tempDir);
        }
    }
}
