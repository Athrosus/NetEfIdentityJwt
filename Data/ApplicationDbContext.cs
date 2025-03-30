using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using IdentityJwtWeather.Data.Models;

namespace IdentityJwtWeather.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<IdentityUser>(options)
    {
        public required DbSet<SolarPowerPlant> SolarPowerPlants { get; set; }
        public required DbSet<SolarPowerPlantProduction> SolarPowerPlantProduction { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
