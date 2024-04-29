using System.ComponentModel.DataAnnotations;

namespace CDRSandbox.Repositories.ClickHouse;

public class DbOptionsClickHouse
{
    [ConfigurationKeyName("ClickHouse")]
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
    
    [Required]
    public string? Username
    {
        get
        {
            return ConnectionString?.Split(';')
                .FirstOrDefault(property =>
                    property.StartsWith("Username=", StringComparison.InvariantCultureIgnoreCase))
                ?.Substring("Username=".Length);
        }
    }
    
    [Required]
    public string? Password
    {
        get
        {
            return ConnectionString?.Split(';')
                .FirstOrDefault(property =>
                    property.StartsWith("Password=", StringComparison.InvariantCultureIgnoreCase))
                ?.Substring("Password=".Length);
        }
    }
}