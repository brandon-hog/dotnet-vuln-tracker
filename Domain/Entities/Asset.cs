namespace Domain.Entities;

public sealed class Asset
{
    public Guid Id { get; private set; }
    public string Hostname { get; private set; }
    public string IpAddress { get; private set; }
    
    // Prevent external code from directly modifying the list
    private readonly List<Vulnerability> _vulnerabilities = [];
    public IReadOnlyCollection<Vulnerability> Vulnerabilities => _vulnerabilities.AsReadOnly();

    public Asset(string hostname, string ipAddress)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hostname);
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAddress);

        Id = Guid.NewGuid();
        Hostname = hostname;
        IpAddress = ipAddress;
    }

    public void AddVulnerability(Vulnerability vulnerability)
    {
        ArgumentNullException.ThrowIfNull(vulnerability);

        if (!_vulnerabilities.Any(v => v.CveId == vulnerability.CveId))
        {
            _vulnerabilities.Add(vulnerability);
        }
    }

    // Domain logic encapsulated within the entity
    public decimal CalculateTotalRiskScore() => 
        _vulnerabilities.Sum(v => v.CvssScore);
}