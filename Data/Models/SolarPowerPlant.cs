using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace IdentityJwtWeather.Data.Models
{
    public class SolarPowerPlant
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(256)]
        public string Name { get; set; } = "";

        [Required]
        [Precision(15, 3)]
        public decimal InstalledPower { get; set; }

        [Required]
        public DateTime DateOfInstallation { get; set; }

        // In real project use SQL native geography type and mapping for EF Core
        [Required]
        [Precision(9, 6)]
        public decimal Latitude { get; set; }

        [Required]
        [Precision(9, 6)]
        public decimal Longitude { get; set; }
    }
}
