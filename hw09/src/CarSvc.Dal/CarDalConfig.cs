using Microsoft.Extensions.Configuration;
using CarSvc.Dal.Repositories;

namespace CarSvc.Dal;

public class CarDalConfig
{
    private const string SECTION_NAME = "CarDal";

    public string? ConnectionString { get; set; }
    public bool AutoMigrate { get; set; }

    public CarDalConfig(IConfiguration configuration)
    {
        configuration.GetSection(SECTION_NAME).Bind(this);
    }
}