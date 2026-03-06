using NuGet.Versioning;

using UpdateCpmVersions;

namespace UpdateCpmVersions.Tests;

public class PackageUpdaterErrorTests
{
    [Test]
    public async Task RunAsync_WithNonExistentPath_ReturnsError()
    {
        var options = new UpdateOptions
        {
            VersionMode = VersionMode.Minor,
            IncludePatterns = [],
            ExcludePatterns = [],
            PinMajorPatterns = [],
            DryRun = false,
        };

        var exitCode = await PackageUpdater.RunAsync(
            "/tmp/this-directory-definitely-does-not-exist-12345/Directory.Packages.props",
            options,
            source: null,
            CancellationToken.None);

        await Assert.That(exitCode).IsEqualTo(1);
    }

    [Test]
    public async Task RunAsync_WithNullPath_FindsDirectoryPackagesProps()
    {
        // Create a temp directory with a Directory.Packages.props file
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var propsPath = Path.Combine(tempDir, "Directory.Packages.props");
            await File.WriteAllTextAsync(propsPath, @"<Project>
  <ItemGroup>
    <PackageVersion Include=""NETStandard.Library"" Version=""2.0.0"" />
  </ItemGroup>
</Project>");

            // Change to the temp directory
            var originalDir = Directory.GetCurrentDirectory();
            try
            {
                Directory.SetCurrentDirectory(tempDir);

                var options = new UpdateOptions
                {
                    VersionMode = VersionMode.Minor,
                    IncludePatterns = [],
                    ExcludePatterns = [],
                    PinMajorPatterns = [],
                    DryRun = true,
                };

                var exitCode = await PackageUpdater.RunAsync(
                    null,
                    options,
                    source: null,
                    CancellationToken.None);

                await Assert.That(exitCode).IsEqualTo(0);
            }
            finally
            {
                Directory.SetCurrentDirectory(originalDir);
            }
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Test]
    public async Task RunAsync_WithEmptyPropsFile_ReturnsSuccess()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var propsPath = Path.Combine(tempDir, "Directory.Packages.props");
            await File.WriteAllTextAsync(propsPath, @"<Project>
  <ItemGroup>
  </ItemGroup>
</Project>");

            var options = new UpdateOptions
            {
                VersionMode = VersionMode.Minor,
                IncludePatterns = [],
                ExcludePatterns = [],
                PinMajorPatterns = [],
                DryRun = true,
            };

            var exitCode = await PackageUpdater.RunAsync(
                propsPath,
                options,
                source: null,
                CancellationToken.None);

            await Assert.That(exitCode).IsEqualTo(0);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Test]
    public async Task RunAsync_WithCancellation_RespectsCancellationToken()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var propsPath = Path.Combine(tempDir, "Directory.Packages.props");
            await File.WriteAllTextAsync(propsPath, @"<Project>
  <ItemGroup>
    <PackageVersion Include=""NETStandard.Library"" Version=""2.0.0"" />
  </ItemGroup>
</Project>");

            var options = new UpdateOptions
            {
                VersionMode = VersionMode.Minor,
                IncludePatterns = [],
                ExcludePatterns = [],
                PinMajorPatterns = [],
                DryRun = true,
            };

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(1));

            await Assert.That(async () => await PackageUpdater.RunAsync(
                propsPath,
                options,
                source: null,
                cts.Token))
                .ThrowsExactly<TaskCanceledException>();
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Test]
    public async Task ClassifyUpdate_WithSameMajorMinorPatch_ReturnsNone()
    {
        var current = NuGetVersion.Parse("1.0.0");
        var candidate = NuGetVersion.Parse("1.0.0");

        var kind = PackageUpdater.ClassifyUpdate(current, candidate);

        await Assert.That(kind).IsEqualTo(UpdateKind.None);
    }

    [Test]
    public async Task ClassifyUpdate_WithPatchIncrement_ReturnsPatch()
    {
        var current = NuGetVersion.Parse("1.0.0");
        var candidate = NuGetVersion.Parse("1.0.1");

        var kind = PackageUpdater.ClassifyUpdate(current, candidate);

        await Assert.That(kind).IsEqualTo(UpdateKind.Patch);
    }

    [Test]
    public async Task ClassifyUpdate_WithMinorIncrement_ReturnsMinor()
    {
        var current = NuGetVersion.Parse("1.0.0");
        var candidate = NuGetVersion.Parse("1.1.0");

        var kind = PackageUpdater.ClassifyUpdate(current, candidate);

        await Assert.That(kind).IsEqualTo(UpdateKind.Minor);
    }

    [Test]
    public async Task ClassifyUpdate_WithMajorIncrement_ReturnsMajor()
    {
        var current = NuGetVersion.Parse("1.0.0");
        var candidate = NuGetVersion.Parse("2.0.0");

        var kind = PackageUpdater.ClassifyUpdate(current, candidate);

        await Assert.That(kind).IsEqualTo(UpdateKind.Major);
    }

    [Test]
    public async Task ClassifyUpdate_WithMultipleChanges_ReturnsMostSignificant()
    {
        var current = NuGetVersion.Parse("1.0.0");
        var candidate = NuGetVersion.Parse("2.3.4");

        var kind = PackageUpdater.ClassifyUpdate(current, candidate);

        await Assert.That(kind).IsEqualTo(UpdateKind.Major);
    }

    [Test]
    public async Task ClassifyUpdate_WithDowngrade_ReturnsMajor()
    {
        var current = NuGetVersion.Parse("2.0.0");
        var candidate = NuGetVersion.Parse("1.0.0");

        var kind = PackageUpdater.ClassifyUpdate(current, candidate);

        await Assert.That(kind).IsEqualTo(UpdateKind.Major);
    }
}
