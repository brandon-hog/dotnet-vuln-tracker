using Domain.Entities;
using Domain.Enums;

namespace Tests;

public class AssetTests
{
    [Fact]
    public void Constructor_ValidInputs_CreatesAsset()
    {
        var asset = new Asset("Web-Server-01", "192.168.1.10");

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
        Assert.ThrowsAny<ArgumentException>(() => new Asset(invalidHostname, ip));
    }

    [Fact]
    public void AddVulnerability_NewCve_AddsToCollection()
    {
        var asset = new Asset("Db-Server", "10.0.0.5");
        var vuln = new Vulnerability("CVE-2024-1234", "SQL Injection", Severity.Critical, 9.8m);

        asset.AddVulnerability(vuln);

        Assert.Single(asset.Vulnerabilities);
        Assert.Contains(vuln, asset.Vulnerabilities);
    }

    [Fact]
    public void AddVulnerability_DuplicateCve_DoesNotAddTwice()
    {
        var asset = new Asset("Db-Server", "10.0.0.5");
        var vuln1 = new Vulnerability("CVE-2024-1234", "SQL Injection", Severity.Critical, 9.8m);
        var vuln2 = new Vulnerability("CVE-2024-1234", "Duplicate Entry", Severity.High, 7.5m);

        asset.AddVulnerability(vuln1);
        asset.AddVulnerability(vuln2); // Same CVE ID

        Assert.Single(asset.Vulnerabilities);
    }

    [Fact]
    public void CalculateTotalRiskScore_MultipleVulnerabilities_ReturnsSum()
    {
        var asset = new Asset("Firewall", "192.168.1.1");
        asset.AddVulnerability(new Vulnerability("CVE-1", "Desc", Severity.Low, 3.5m));
        asset.AddVulnerability(new Vulnerability("CVE-2", "Desc", Severity.Medium, 5.0m));

        var totalScore = asset.CalculateTotalRiskScore();

        Assert.Equal(8.5m, totalScore);
    }
}
