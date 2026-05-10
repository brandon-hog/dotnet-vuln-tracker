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

    public static bool TryParse(string? value, out CPE? parsed)
    {
        parsed = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var components = value.Split(':');
        if (components.Length != 13)
        {
            return false;
        }

        if (!string.Equals(components[0], "cpe", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(components[1], "2.3", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (!TryParsePart(components[2], out var part))
        {
            return false;
        }

        try
        {
            parsed = new CPE
            {
                Part = part,
                Vendor = components[3],
                Product = components[4],
                Version = components[5],
                Update = components[6],
                Language = components[8],
                SwEdition = components[9],
                TargetSw = components[10],
                TargetHw = components[11]
            };

            return true;
        }
        catch (ArgumentException)
        {
            parsed = null;
            return false;
        }
    }

    public static implicit operator string(CPE cpe)
    {
        ArgumentNullException.ThrowIfNull(cpe);
        return cpe.ToString();
    }

    private static bool TryParsePart(string value, out CpePart part)
    {
        part = CpePart.Application;

        if (string.Equals(value, "a", StringComparison.OrdinalIgnoreCase))
        {
            part = CpePart.Application;
            return true;
        }

        if (string.Equals(value, "o", StringComparison.OrdinalIgnoreCase))
        {
            part = CpePart.OperatingSystem;
            return true;
        }

        if (string.Equals(value, "h", StringComparison.OrdinalIgnoreCase))
        {
            part = CpePart.Hardware;
            return true;
        }

        return false;
    }
}
