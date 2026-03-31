using System.ComponentModel.DataAnnotations;

namespace Shared.Dtos;

public class UpdateAssetDto
{
    [Required(ErrorMessage = "Hostname is required.")]
    [StringLength(255, MinimumLength = 2, ErrorMessage = "Hostname must be between 2 and 255 characters.")]
    public string Hostname { get; set; } = string.Empty;

    [Required(ErrorMessage = "IP Address is required.")]
    [RegularExpression(@"^(?:[0-9]{1,3}\.){3}[0-9]{1,3}$", ErrorMessage = "Must be a valid IPv4 address.")]
    public string IpAddress { get; set; } = string.Empty;
}
