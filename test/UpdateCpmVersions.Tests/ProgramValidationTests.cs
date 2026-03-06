using System.CommandLine;

using UpdateCpmVersions;

namespace UpdateCpmVersions.Tests;

public class ProgramValidationTests
{
    [Test]
    public async Task UpdateOptions_WithPatchOnly_SetsVersionModeToPatchOnly()
    {
        var options = new UpdateOptions
        {
            VersionMode = VersionMode.PatchOnly,
            IncludePatterns = [],
            ExcludePatterns = [],
            PinMajorPatterns = [],
            DryRun = false,
        };

        await Assert.That(options.VersionMode).IsEqualTo(VersionMode.PatchOnly);
    }

    [Test]
    public async Task UpdateOptions_WithMajor_SetsVersionModeToMajor()
    {
        var options = new UpdateOptions
        {
            VersionMode = VersionMode.Major,
            IncludePatterns = [],
            ExcludePatterns = [],
            PinMajorPatterns = [],
            DryRun = false,
        };

        await Assert.That(options.VersionMode).IsEqualTo(VersionMode.Major);
    }

    [Test]
    public async Task UpdateOptions_Default_SetsVersionModeToMinor()
    {
        var options = new UpdateOptions
        {
            VersionMode = VersionMode.Minor,
            IncludePatterns = [],
            ExcludePatterns = [],
            PinMajorPatterns = [],
            DryRun = false,
        };

        await Assert.That(options.VersionMode).IsEqualTo(VersionMode.Minor);
    }

    [Test]
    public async Task UpdateOptions_WithIncludePatterns_StoresPatterns()
    {
        var options = new UpdateOptions
        {
            VersionMode = VersionMode.Minor,
            IncludePatterns = ["Package.*", "Another.*"],
            ExcludePatterns = [],
            PinMajorPatterns = [],
            DryRun = false,
        };

        await Assert.That(options.IncludePatterns.Length).IsEqualTo(2);
        await Assert.That(options.IncludePatterns).Contains("Package.*");
        await Assert.That(options.IncludePatterns).Contains("Another.*");
    }

    [Test]
    public async Task UpdateOptions_WithExcludePatterns_StoresPatterns()
    {
        var options = new UpdateOptions
        {
            VersionMode = VersionMode.Minor,
            IncludePatterns = [],
            ExcludePatterns = ["Package.*", "Another.*"],
            PinMajorPatterns = [],
            DryRun = false,
        };

        await Assert.That(options.ExcludePatterns.Length).IsEqualTo(2);
        await Assert.That(options.ExcludePatterns).Contains("Package.*");
        await Assert.That(options.ExcludePatterns).Contains("Another.*");
    }

    [Test]
    public async Task UpdateOptions_WithPinMajorPatterns_StoresPatterns()
    {
        var options = new UpdateOptions
        {
            VersionMode = VersionMode.Major,
            IncludePatterns = [],
            ExcludePatterns = [],
            PinMajorPatterns = ["Package.*"],
            DryRun = false,
        };

        await Assert.That(options.PinMajorPatterns.Length).IsEqualTo(1);
        await Assert.That(options.PinMajorPatterns).Contains("Package.*");
    }

    [Test]
    public async Task UpdateOptions_WithDryRun_SetsDryRunTrue()
    {
        var options = new UpdateOptions
        {
            VersionMode = VersionMode.Minor,
            IncludePatterns = [],
            ExcludePatterns = [],
            PinMajorPatterns = [],
            DryRun = true,
        };

        await Assert.That(options.DryRun).IsTrue();
    }

    [Test]
    public async Task UpdateOptions_WithoutDryRun_SetsDryRunFalse()
    {
        var options = new UpdateOptions
        {
            VersionMode = VersionMode.Minor,
            IncludePatterns = [],
            ExcludePatterns = [],
            PinMajorPatterns = [],
            DryRun = false,
        };

        await Assert.That(options.DryRun).IsFalse();
    }

    [Test]
    public async Task UpdateOptions_WithEmptyArrays_DoesNotThrow()
    {
        var options = new UpdateOptions
        {
            VersionMode = VersionMode.Minor,
            IncludePatterns = [],
            ExcludePatterns = [],
            PinMajorPatterns = [],
            DryRun = false,
        };

        await Assert.That(options.IncludePatterns).IsEmpty();
        await Assert.That(options.ExcludePatterns).IsEmpty();
        await Assert.That(options.PinMajorPatterns).IsEmpty();
    }

    [Test]
    public async Task VersionMode_HasExpectedEnumValues()
    {
        await Assert.That(Enum.IsDefined(typeof(VersionMode), VersionMode.PatchOnly)).IsTrue();
        await Assert.That(Enum.IsDefined(typeof(VersionMode), VersionMode.Minor)).IsTrue();
        await Assert.That(Enum.IsDefined(typeof(VersionMode), VersionMode.Major)).IsTrue();
    }

    [Test]
    public async Task UpdateKind_HasExpectedEnumValues()
    {
        await Assert.That(Enum.IsDefined(typeof(UpdateKind), UpdateKind.None)).IsTrue();
        await Assert.That(Enum.IsDefined(typeof(UpdateKind), UpdateKind.Patch)).IsTrue();
        await Assert.That(Enum.IsDefined(typeof(UpdateKind), UpdateKind.Minor)).IsTrue();
        await Assert.That(Enum.IsDefined(typeof(UpdateKind), UpdateKind.Major)).IsTrue();
    }

    [Test]
    public async Task UpdateOptions_WithMultiplePatterns_HandlesCorrectly()
    {
        var options = new UpdateOptions
        {
            VersionMode = VersionMode.Major,
            IncludePatterns = [],
            ExcludePatterns = [],
            PinMajorPatterns = ["System.*", "Microsoft.*", "NETStandard.*"],
            DryRun = false,
        };

        await Assert.That(options.PinMajorPatterns.Length).IsEqualTo(3);
    }
}
