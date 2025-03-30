using IdentityJwtWeather.Data.Models;
using IdentityJwtWeather.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace IdentityJwtWeather.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolarPowerPlantsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SolarPowerPlantsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/SolarPowerPlant
        // Retrieves all solar power plants.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SolarPowerPlant>>> GetAll()
        {
            return await _context.SolarPowerPlants.ToListAsync();
        }

        // GET: api/SolarPowerPlant/5
        // Retrieves a specific solar power plant by ID.
        [HttpGet("{id}")]
        public async Task<ActionResult<SolarPowerPlant>> Get(int id)
        {
            var plant = await _context.SolarPowerPlants.FindAsync(id);
            if (plant == null)
            {
                return NotFound();
            }
            return plant;
        }

        // POST: api/SolarPowerPlant
        // Creates a new solar power plant.
        [HttpPost]
        [Authorize]
        [HttpPost]
        public async Task<ActionResult<SolarPowerPlant>> Create(SolarPowerPlantObject plantObject)
        {
            var plant = new SolarPowerPlant
            {
                Name = plantObject.Name,
                InstalledPower = plantObject.InstalledPower,
                DateOfInstallation = plantObject.DateOfInstallation,
                Latitude = plantObject.Latitude,
                Longitude = plantObject.Longitude
            };

            _context.SolarPowerPlants.Add(plant);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = plant.Id }, plant);
        }

        // PUT: api/SolarPowerPlant/5
        // Updates an existing solar power plant.
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<SolarPowerPlant>> Update(int id, SolarPowerPlantObject plantObject)
        {
            var plant = await _context.SolarPowerPlants.FindAsync(id);
            if (plant == null)
            {
                return NotFound();
            }

            plant.Name = plantObject.Name;
            plant.InstalledPower = plantObject.InstalledPower;
            plant.DateOfInstallation = plantObject.DateOfInstallation;
            plant.Latitude = plantObject.Latitude;
            plant.Longitude = plantObject.Longitude;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SolarPowerPlantExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return Ok();
        }

        // DELETE: api/SolarPowerPlant/5
        // Deletes an existing solar power plant.
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var plant = await _context.SolarPowerPlants.FindAsync(id);
            if (plant == null)
            {
                return NotFound();
            }

            _context.SolarPowerPlants.Remove(plant);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // Helper method to check if a plant exists.
        private bool SolarPowerPlantExists(int id)
        {
            return _context.SolarPowerPlants.Any(e => e.Id == id);
        }

        public class SolarPowerPlantObject
        {
            public string Name { get; set; } = "";
            public decimal InstalledPower { get; set; }
            public DateTime DateOfInstallation { get; set; } = DateTime.Now;
            public decimal Latitude { get; set; }
            public decimal Longitude { get; set; }
        }
    }
}
