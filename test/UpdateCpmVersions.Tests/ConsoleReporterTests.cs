using NuGet.Versioning;

using UpdateCpmVersions;

namespace UpdateCpmVersions.Tests;

public class ConsoleReporterTests
{
    [Test]
    public async Task Report_WithNoUpdatesAndNoSkipped_DoesNotThrow()
    {
        // Just verify the method doesn't throw - console output testing is fragile with TUnit
        ConsoleReporter.Report([], [], dryRun: false, "/path/to/Directory.Packages.props");
        await Assert.That(true).IsTrue();
    }

    [Test]
    public async Task Report_WithUpdates_DoesNotThrow()
    {
        var updates = new List<PackageUpdate>
        {
            new("Package.A", NuGetVersion.Parse("1.0.0"), NuGetVersion.Parse("1.0.1"), UpdateKind.Patch),
            new("Package.B", NuGetVersion.Parse("2.0.0"), NuGetVersion.Parse("2.1.0"), UpdateKind.Minor),
            new("Package.C", NuGetVersion.Parse("3.0.0"), NuGetVersion.Parse("4.0.0"), UpdateKind.Major),
        };

        ConsoleReporter.Report(updates, [], dryRun: false, "/path/to/Directory.Packages.props");
        await Assert.That(true).IsTrue();
    }

    [Test]
    public async Task Report_WithDryRun_DoesNotThrow()
    {
        var updates = new List<PackageUpdate>
        {
            new("Package.A", NuGetVersion.Parse("1.0.0"), NuGetVersion.Parse("1.0.1"), UpdateKind.Patch),
        };

        ConsoleReporter.Report(updates, [], dryRun: true, "/path/to/Directory.Packages.props");
        await Assert.That(true).IsTrue();
    }

    [Test]
    public async Task Report_WithoutDryRun_DoesNotThrow()
    {
        var updates = new List<PackageUpdate>
        {
            new("Package.A", NuGetVersion.Parse("1.0.0"), NuGetVersion.Parse("1.0.1"), UpdateKind.Patch),
        };

        ConsoleReporter.Report(updates, [], dryRun: false, "/path/to/Directory.Packages.props");
        await Assert.That(true).IsTrue();
    }

    [Test]
    public async Task Report_WithSkippedUpdates_DoesNotThrow()
    {
        var skipped = new List<SkippedUpdate>
        {
            new("Package.X", NuGetVersion.Parse("1.0.0"), NuGetVersion.Parse("2.0.0"), UpdateKind.Major, "would be major"),
        };

        ConsoleReporter.Report([], skipped, dryRun: false, "/path/to/Directory.Packages.props");
        await Assert.That(true).IsTrue();
    }

    [Test]
    public async Task Report_WithMultipleUpdatesAndSkipped_DoesNotThrow()
    {
        var updates = new List<PackageUpdate>
        {
            new("Zebra.Package", NuGetVersion.Parse("1.0.0"), NuGetVersion.Parse("1.0.1"), UpdateKind.Patch),
            new("Alpha.Package", NuGetVersion.Parse("1.0.0"), NuGetVersion.Parse("1.0.1"), UpdateKind.Patch),
        };

        var skipped = new List<SkippedUpdate>
        {
            new("Beta.Package", NuGetVersion.Parse("1.0.0"), NuGetVersion.Parse("2.0.0"), UpdateKind.Major, "would be major"),
        };

        ConsoleReporter.Report(updates, skipped, dryRun: false, "/path/to/Directory.Packages.props");
        await Assert.That(true).IsTrue();
    }

    [Test]
    public async Task Info_DoesNotThrow()
    {
        ConsoleReporter.Info("Test message");
        await Assert.That(true).IsTrue();
    }

    [Test]
    public async Task Warning_DoesNotThrow()
    {
        ConsoleReporter.Warning("Warning message");
        await Assert.That(true).IsTrue();
    }

    [Test]
    public async Task Error_DoesNotThrow()
    {
        ConsoleReporter.Error("Error message");
        await Assert.That(true).IsTrue();
    }

    [Test]
    public async Task Report_WithEmptyPackageId_DoesNotThrow()
    {
        var updates = new List<PackageUpdate>
        {
            new("", NuGetVersion.Parse("1.0.0"), NuGetVersion.Parse("1.0.1"), UpdateKind.Patch),
        };

        ConsoleReporter.Report(updates, [], dryRun: false, "/path/to/Directory.Packages.props");
        await Assert.That(true).IsTrue();
    }

    [Test]
    public async Task Report_WithManyUpdates_DoesNotThrow()
    {
        var updates = new List<PackageUpdate>();
        for (int i = 0; i < 100; i++)
        {
            updates.Add(new($"Package.{i}", NuGetVersion.Parse("1.0.0"), NuGetVersion.Parse("1.0.1"), UpdateKind.Patch));
        }

        ConsoleReporter.Report(updates, [], dryRun: false, "/path/to/Directory.Packages.props");
        await Assert.That(true).IsTrue();
    }
}
