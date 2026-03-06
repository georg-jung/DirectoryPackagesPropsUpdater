using NuGet.Versioning;

using UpdateCpmVersions;

namespace UpdateCpmVersions.Tests;

public class NuGetClientTests
{
    [Test]
    public async Task GetVersionsAsync_ReturnsVersionsForKnownPackage()
    {
        using var client = new NuGetClient();
        var versions = await client.GetVersionsAsync("NETStandard.Library");

        await Assert.That(versions.Count).IsGreaterThan(0);
        await Assert.That(versions).Contains(NuGetVersion.Parse("1.6.0"));
        await Assert.That(versions).Contains(NuGetVersion.Parse("2.0.3"));
    }

    [Test]
    public async Task GetVersionsAsync_ReturnsEmptyForNonexistentPackage()
    {
        using var client = new NuGetClient();
        var versions = await client.GetVersionsAsync(
            "This.Package.Should.Never.Exist.On.NuGet.12345");

        await Assert.That(versions).IsEmpty();
    }

    [Test]
    public async Task GetAllVersionsAsync_FetchesMultiplePackages()
    {
        using var client = new NuGetClient();
        var result = await client.GetAllVersionsAsync(
            ["NETStandard.Library", "Microsoft.NETCore.Platforms"]);

        await Assert.That(result).Count().IsEqualTo(2);
        await Assert.That(result["NETStandard.Library"].Count).IsGreaterThan(0);
        await Assert.That(result["Microsoft.NETCore.Platforms"].Count).IsGreaterThan(0);
    }

    [Test]
    public async Task GetVersionsAsync_WithCustomHttpClient_DoesNotThrow()
    {
        using var httpClient = new HttpClient();
        using var client = new NuGetClient(http: httpClient);
        var versions = await client.GetVersionsAsync("NETStandard.Library");

        await Assert.That(versions.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task GetVersionsAsync_WithCancellationToken_CanBeCanceled()
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromMilliseconds(1));

        using var client = new NuGetClient();

        await Assert.That(async () => await client.GetVersionsAsync("NETStandard.Library", cts.Token))
            .ThrowsExactly<TaskCanceledException>();
    }

    [Test]
    public async Task GetAllVersionsAsync_WithEmptyList_ReturnsEmptyDictionary()
    {
        using var client = new NuGetClient();
        var result = await client.GetAllVersionsAsync([]);

        await Assert.That(result).IsEmpty();
    }

    [Test]
    public async Task GetAllVersionsAsync_WithSinglePackage_ReturnsOneDictionary()
    {
        using var client = new NuGetClient();
        var result = await client.GetAllVersionsAsync(["NETStandard.Library"]);

        await Assert.That(result).Count().IsEqualTo(1);
        await Assert.That(result["NETStandard.Library"].Count).IsGreaterThan(0);
    }

    [Test]
    public async Task GetAllVersionsAsync_WithMaxConcurrency_DoesNotThrow()
    {
        using var client = new NuGetClient();
        var result = await client.GetAllVersionsAsync(
            ["NETStandard.Library", "Microsoft.NETCore.Platforms"],
            maxConcurrency: 1);

        await Assert.That(result).Count().IsEqualTo(2);
    }

    [Test]
    public async Task GetVersionsAsync_WithLowercaseAndUppercasePackageId_ReturnsSameResults()
    {
        using var client = new NuGetClient();
        var lowerVersions = await client.GetVersionsAsync("netstandard.library");
        var upperVersions = await client.GetVersionsAsync("NETSTANDARD.LIBRARY");

        await Assert.That(lowerVersions.Count).IsGreaterThan(0);
        await Assert.That(upperVersions.Count).IsGreaterThan(0);
        await Assert.That(lowerVersions.Count).IsEqualTo(upperVersions.Count);
    }

    [Test]
    public async Task ResolveBaseUrlAsync_WithNuGetOrgServiceIndex_ReturnsValidUrl()
    {
        var baseUrl = await NuGetClient.ResolveBaseUrlAsync("https://api.nuget.org/v3/index.json");

        await Assert.That(baseUrl.ToString()).IsNotEmpty();
        await Assert.That(baseUrl.ToString()).Contains("nuget.org");
    }

    [Test]
    public async Task ResolveBaseUrlAsync_WithInvalidServiceIndex_Throws()
    {
        await Assert.That(async () => await NuGetClient.ResolveBaseUrlAsync("https://httpbin.org/status/404"))
            .ThrowsException();
    }

    [Test]
    public async Task ResolveBaseUrlAsync_WithCustomHttpClient_DoesNotDisposeIt()
    {
        using var httpClient = new HttpClient();
        var baseUrl = await NuGetClient.ResolveBaseUrlAsync(
            "https://api.nuget.org/v3/index.json",
            http: httpClient);

        await Assert.That(baseUrl.ToString()).IsNotEmpty();

        // HttpClient should still be usable
        var response = await httpClient.GetAsync("https://api.nuget.org/v3/index.json");
        await Assert.That(response.IsSuccessStatusCode).IsTrue();
    }

    [Test]
    public async Task GetVersionsAsync_WithCustomBaseUrl_DoesNotThrow()
    {
        var customBaseUrl = new Uri("https://api.nuget.org/v3-flatcontainer/");
        using var client = new NuGetClient(baseUrl: customBaseUrl);
        var versions = await client.GetVersionsAsync("NETStandard.Library");

        await Assert.That(versions.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task NuGetClient_CanBeDisposedMultipleTimes()
    {
        var client = new NuGetClient();
        client.Dispose();
        client.Dispose(); // Should not throw

        await Assert.That(true).IsTrue();
    }

    [Test]
    public async Task GetAllVersionsAsync_WithDuplicatePackageIds_HandlesCorrectly()
    {
        using var client = new NuGetClient();
        var result = await client.GetAllVersionsAsync(
            ["NETStandard.Library", "NETStandard.Library"]);

        // Dictionary should only have one entry due to string comparer
        await Assert.That(result).Count().IsEqualTo(1);
        await Assert.That(result["NETStandard.Library"].Count).IsGreaterThan(0);
    }

    [Test]
    public async Task GetAllVersionsAsync_IsCaseInsensitive()
    {
        using var client = new NuGetClient();
        var result = await client.GetAllVersionsAsync(
            ["NETStandard.Library", "netstandard.library"]);

        // Dictionary uses case-insensitive comparer, so should have one entry
        await Assert.That(result).Count().IsEqualTo(1);
    }
}
