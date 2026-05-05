namespace Shared;

/// <summary>
/// CPE 2.3 part component (first attribute after <c>cpe:2.3</c>).
/// </summary>
public enum CpePart
{
    /// <summary>Operating system — binding character <c>o</c>.</summary>
    OperatingSystem,

    /// <summary>Hardware — binding character <c>h</c>.</summary>
    Hardware,

    /// <summary>Application — binding character <c>a</c>.</summary>
    Application,
}

/// <summary>
/// Represents a CPE 2.3 formatted string as structured components.
/// Components are joined as-is; reserved characters in vendor/product text require
/// NIST CPE escaping before use in NVD-grade matching.
/// </summary>
public sealed class CPE
{
    private string _language = "*";

    public CpePart Part { get; set; }

    public string Vendor { get; set; } = string.Empty;

    public string Product { get; set; } = string.Empty;

    public string Version { get; set; } = string.Empty;

    public string Update { get; set; } = string.Empty;

    /// <summary>Deprecated component; always <c>*</c> in the binding string.</summary>
    public string Edition => "*";

    /// <summary>Must be <c>*</c> or <c>en-us</c> (assignments normalize <c>en-us</c> casing).</summary>
    public string Language
    {
        get => _language;
        set
        {
            if (value == "*")
            {
                _language = "*";
                return;
            }

            if (string.Equals(value, "en-us", StringComparison.OrdinalIgnoreCase))
            {
                _language = "en-us";
                return;
            }

            throw new ArgumentException("Language must be \"*\" or \"en-us\".", nameof(value));
        }
    }

    public string SwEdition { get; set; } = string.Empty;

    public string TargetSw { get; set; } = string.Empty;

    public string TargetHw { get; set; } = string.Empty;

    /// <summary>Fixed to <c>*</c> for now.</summary>
    public string Other => "*";

    public override string ToString()
    {
        var part = Part switch
        {
            CpePart.OperatingSystem => "o",
            CpePart.Hardware => "h",
            CpePart.Application => "a",
            _ => throw new ArgumentOutOfRangeException(nameof(Part)),
        };

        return string.Join(
            ":",
            "cpe",
            "2.3",
            part,
            Vendor,
            Product,
            Version,
            Update,
            Edition,
            Language,
            SwEdition,
            TargetSw,
            TargetHw,
            Other);
    }

    public static implicit operator string(CPE cpe)
    {
        ArgumentNullException.ThrowIfNull(cpe);
        return cpe.ToString();
    }
}
