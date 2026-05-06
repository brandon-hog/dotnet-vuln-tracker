using Domain.Entities;

namespace Tests;

public class AssetTests
{
    private const string DummyCpe = "cpe:2.3:a:vendor:product:1.0:*:*:en-us:*:*:*:*";

    private static Vulnerability MakeVuln(string id, decimal? score = null, string severity = "LOW") =>
        new()
        {
            Id = id,
            VulnStatus = "Analyzed",
            CvssV31BaseScore = score,
            CvssV31BaseSeverity = severity,
        };

    [Fact]
    public void Constructor_ValidInputs_CreatesAsset()
    {
        var asset = new Asset("Web-Server-01", "192.168.1.10", DummyCpe);

        Assert.NotEqual(Guid.Empty, asset.Id);
        Assert.Equal("Web-Server-01", asset.Hostname);
        Assert.Equal("192.168.1.10", asset.IpAddress);
        Assert.Empty(asset.Vulnerabilities);
    }

    [Theory]
    [InlineData("", "192.168.1.10")]
    [InlineData(" ", "192.168.1.10")]
    public void Constructor_InvalidHostname_ThrowsArgumentException(string invalidHostname, string ip)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Asset(invalidHostname, ip, DummyCpe));
    }

    [Fact]
    public void AddVulnerability_NewCve_AddsToCollection()
    {
        var asset = new Asset("Db-Server", "10.0.0.5", DummyCpe);
        var vuln = MakeVuln("CVE-2024-1234", 9.8m, "CRITICAL");

        asset.AddVulnerability(vuln);

        Assert.Single(asset.Vulnerabilities);
        Assert.Contains(vuln, asset.Vulnerabilities);
    }

    [Fact]
    public void AddVulnerability_DuplicateCve_DoesNotAddTwice()
    {
        var asset = new Asset("Db-Server", "10.0.0.5", DummyCpe);
        var vuln1 = MakeVuln("CVE-2024-1234", 9.8m, "CRITICAL");
        var vuln2 = MakeVuln("CVE-2024-1234", 7.5m, "HIGH");

        asset.AddVulnerability(vuln1);
        asset.AddVulnerability(vuln2); // Same CVE ID

        Assert.Single(asset.Vulnerabilities);
    }

    [Fact]
    public void CalculateTotalRiskScore_MultipleVulnerabilities_ReturnsSum()
    {
        var asset = new Asset("Firewall", "192.168.1.1", DummyCpe);
        asset.AddVulnerability(MakeVuln("CVE-1", 3.5m));
        asset.AddVulnerability(MakeVuln("CVE-2", 5.0m, "MEDIUM"));

        var totalScore = asset.CalculateTotalRiskScore();

        Assert.Equal(8.5m, totalScore);
    }

    [Fact]
    public void ReplaceVulnerabilities_OverwritesExistingSet()
    {
        var asset = new Asset("Cache", "10.0.0.7", DummyCpe);
        asset.AddVulnerability(MakeVuln("CVE-OLD-1", 4.0m));
        asset.AddVulnerability(MakeVuln("CVE-OLD-2", 6.0m));

        var replacement = new[]
        {
            MakeVuln("CVE-NEW-1", 7.0m, "HIGH"),
            MakeVuln("CVE-NEW-1", 7.0m, "HIGH"), // Duplicate id should be deduped
            MakeVuln("CVE-NEW-2", 2.0m),
        };

        asset.ReplaceVulnerabilities(replacement);

        Assert.Equal(2, asset.Vulnerabilities.Count);
        Assert.DoesNotContain(asset.Vulnerabilities, v => v.Id == "CVE-OLD-1");
        Assert.DoesNotContain(asset.Vulnerabilities, v => v.Id == "CVE-OLD-2");
        Assert.Contains(asset.Vulnerabilities, v => v.Id == "CVE-NEW-1");
        Assert.Contains(asset.Vulnerabilities, v => v.Id == "CVE-NEW-2");
    }
}
