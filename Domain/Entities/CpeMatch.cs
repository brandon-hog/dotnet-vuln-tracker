namespace Domain.Entities;

using System.ComponentModel.DataAnnotations;

public class CpeMatch
{
    [Key]
    public long Id { get; set; }

    public string VulnerabilityId { get; set; } = null!;
    public Vulnerability Vulnerability { get; set; } = null!;

    // The CPE 2.3 String (cpe:2.3:a:...)
    public string Criteria { get; set; } = null!;
    
    // NVD's unique UUID for this CPE
    public string MatchCriteriaId { get; set; } = null!;
    public bool Vulnerable { get; set; }
}