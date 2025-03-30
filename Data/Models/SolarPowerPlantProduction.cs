using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices;

namespace IdentityJwtWeather.Data.Models
{
    public class SolarPowerPlantProduction
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(SolarPlant))]
        public int SolarPlantId { get; set; }
        public SolarPowerPlant SolarPlant { get; set; } = null!;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Precision(15, 3)]
        public decimal Production { get; set; }
    }
}
