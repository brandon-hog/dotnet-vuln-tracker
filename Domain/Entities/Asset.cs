namespace Domain.Entities;

public sealed class Asset
{
    public Guid Id { get; private set; }
    public string Hostname { get; private set; }
    public string IpAddress { get; private set; }
    public string Cpe { get; private set; }
    
    // Prevent external code from directly modifying the list
    private readonly List<Vulnerability> _vulnerabilities = [];
    public IReadOnlyCollection<Vulnerability> Vulnerabilities => _vulnerabilities.AsReadOnly();

    public Asset(string hostname, string ipAddress, string cpe)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hostname);
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAddress);

        Id = Guid.NewGuid();
        Hostname = hostname;
        IpAddress = ipAddress;
        Cpe = cpe;
    }

    public void UpdateDetails(string hostname, string ipAddress, string cpe)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hostname);
        ArgumentException.ThrowIfNullOrWhiteSpace(ipAddress);

        Hostname = hostname;
        IpAddress = ipAddress;
        Cpe = cpe;
    }

    public void AddVulnerability(Vulnerability vulnerability)
    {
        ArgumentNullException.ThrowIfNull(vulnerability);

        if (!_vulnerabilities.Any(v => v.Id == vulnerability.Id))
        {
            _vulnerabilities.Add(vulnerability);
        }
    }

    // Domain logic encapsulated within the entity
    public decimal CalculateTotalRiskScore() => 
        _vulnerabilities.Sum(v => v.CvssV31BaseScore ?? 0);
}