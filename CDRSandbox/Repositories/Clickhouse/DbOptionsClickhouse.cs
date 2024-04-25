using System.ComponentModel.DataAnnotations;

namespace CDRSandbox.Repositories.Clickhouse;

public class DbOptionsClickhouse
{
    [ConfigurationKeyName("Clickhouse")]
    [RegularExpression("^[a-zA-Z0-9=.;]+$")]
    [Required]
    public string? ConnectionString { get; set; }

    [Required]
    public string? Database
    {
        get
        {
            return ConnectionString?.Split(';')
                .FirstOrDefault(property =>
                    property.StartsWith("Database=", StringComparison.InvariantCultureIgnoreCase))
                ?.Substring("Database=".Length);
        }
    }
}