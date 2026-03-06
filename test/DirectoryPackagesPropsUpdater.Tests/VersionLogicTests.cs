using NuGet.Versioning;

using DirectoryPackagesPropsUpdater;

namespace DirectoryPackagesPropsUpdater.Tests;

public class VersionLogicTests
{
    private static List<NuGetVersion> V(params string[] versions)
        => versions.Select(NuGetVersion.Parse).ToList();

    [Test]
    public async Task FindBestUpdate_Minor_StaysInMajorBand()
    {
        var current = NuGetVersion.Parse("2.1.0");
        var available = V("1.0.0", "2.0.0", "2.1.0", "2.2.0", "2.3.0", "3.0.0");

        var result = PackageUpdater.FindBestUpdate(current, available, VersionMode.Minor);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!).IsEqualTo(NuGetVersion.Parse("2.3.0"));
    }

    [Test]
    public async Task FindBestUpdate_Minor_DoesNotCrossMajor()
    {
        var current = NuGetVersion.Parse("2.3.0");
        var available = V("2.3.0", "3.0.0", "4.0.0");

        var result = PackageUpdater.FindBestUpdate(current, available, VersionMode.Minor);

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task FindBestUpdate_PatchOnly_StaysInMinorBand()
    {
        var current = NuGetVersion.Parse("2.1.0");
        var available = V("2.1.0", "2.1.1", "2.1.5", "2.2.0", "3.0.0");

        var result = PackageUpdater.FindBestUpdate(current, available, VersionMode.PatchOnly);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!).IsEqualTo(NuGetVersion.Parse("2.1.5"));
    }

    [Test]
    public async Task FindBestUpdate_PatchOnly_DoesNotCrossMinor()
    {
        var current = NuGetVersion.Parse("2.1.5");
        var available = V("2.1.5", "2.2.0", "3.0.0");

        var result = PackageUpdater.FindBestUpdate(current, available, VersionMode.PatchOnly);

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task FindBestUpdate_Major_ReturnsHighest()
    {
        var current = NuGetVersion.Parse("2.1.0");
        var available = V("2.1.0", "2.2.0", "3.0.0", "4.0.0-beta", "4.0.0");

        var result = PackageUpdater.FindBestUpdate(current, available, VersionMode.Major);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!).IsEqualTo(NuGetVersion.Parse("4.0.0"));
    }

    [Test]
    public async Task FindBestUpdate_ReturnsNull_WhenAlreadyLatest()
    {
        var current = NuGetVersion.Parse("3.0.0");
        var available = V("1.0.0", "2.0.0", "3.0.0");

        var result = PackageUpdater.FindBestUpdate(current, available, VersionMode.Major);

        await Assert.That(result).IsNull();
    }

    [Test]
    [Arguments("1.0.0", "2.0.0", "Major")]
    [Arguments("1.0.0", "1.1.0", "Minor")]
    [Arguments("1.0.0", "1.0.1", "Patch")]
    [Arguments("1.0.0", "1.0.0", "None")]
    public async Task ClassifyUpdate_CategorizesCorrectly(
        string current, string updated, string expectedKind)
    {
        var expected = Enum.Parse<UpdateKind>(expectedKind);
        var result = PackageUpdater.ClassifyUpdate(
            NuGetVersion.Parse(current), NuGetVersion.Parse(updated));

        await Assert.That(result).IsEqualTo(expected);
    }

    [Test]
    public async Task ClassifyUpdate_PrereleaseToStable_IsPatch()
    {
        var result = PackageUpdater.ClassifyUpdate(
            NuGetVersion.Parse("2.0.0-rc.1"), NuGetVersion.Parse("2.0.0"));

        await Assert.That(result).IsEqualTo(UpdateKind.Patch);
    }

    [Test]
    public async Task ClassifyUpdate_FourPartRevisionDifference_IsPatch()
    {
        // 4-part versions like StyleCop.Analyzers.Unstable uses (1.2.0.435 -> 1.2.0.507)
        var result = PackageUpdater.ClassifyUpdate(
            NuGetVersion.Parse("1.2.0.435"), NuGetVersion.Parse("1.2.0.507"));

        await Assert.That(result).IsEqualTo(UpdateKind.Patch);
    }

    [Test]
    public async Task ClassifyUpdate_SameFourPartVersion_IsNone()
    {
        var result = PackageUpdater.ClassifyUpdate(
            NuGetVersion.Parse("1.2.0.507"), NuGetVersion.Parse("1.2.0.507"));

        await Assert.That(result).IsEqualTo(UpdateKind.None);
    }

    [Test]
    public async Task ClassifyUpdate_PrereleaseBump_IsPatch()
    {
        // Different prerelease labels, same M.m.p
        var result = PackageUpdater.ClassifyUpdate(
            NuGetVersion.Parse("1.2.0-beta.205"), NuGetVersion.Parse("1.2.0-beta.507"));

        await Assert.That(result).IsEqualTo(UpdateKind.Patch);
    }

    [Test]
    public async Task FindBestUpdate_PrereleaseHigherThanAllStable_ReturnsNull()
    {
        // Simulates StyleCop.Analyzers: 1.2.0-beta.507 is higher than all stable (1.1.118)
        var current = NuGetVersion.Parse("1.2.0-beta.507");
        var stableOnly = V("1.0.0", "1.0.2", "1.1.0", "1.1.118");

        var result = PackageUpdater.FindBestUpdate(current, stableOnly, VersionMode.Minor);

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task FindBestUpdate_FourPartVersions_UpdatesWithinBand()
    {
        var current = NuGetVersion.Parse("1.2.0.435");
        var available = V("1.2.0.435", "1.2.0.507", "1.2.0.556");

        var result = PackageUpdater.FindBestUpdate(current, available, VersionMode.PatchOnly);

        await Assert.That(result).IsNotNull();
        await Assert.That(result!).IsEqualTo(NuGetVersion.Parse("1.2.0.556"));
    }

    [Test]
    public async Task FindBestUpdate_SkipsPrereleaseInCandidates()
    {
        // Caller filters prereleases, but let's verify behavior if they sneak in
        var current = NuGetVersion.Parse("1.0.0");
        var available = V("1.0.0", "1.1.0", "1.2.0-beta.507");

        var result = PackageUpdater.FindBestUpdate(current, available, VersionMode.Minor);

        // 1.2.0-beta.507 > 1.1.0, but our caller should filter it.
        // FindBestUpdate itself doesn't filter, so it returns the prerelease.
        await Assert.That(result).IsNotNull();
        await Assert.That(result!).IsEqualTo(NuGetVersion.Parse("1.2.0-beta.507"));
    }
}
