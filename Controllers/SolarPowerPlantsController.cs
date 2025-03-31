using IdentityJwtWeather.Data.Models;
using IdentityJwtWeather.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Azure.Core;
using System.Text.Json;

namespace IdentityJwtWeather.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolarPowerPlantsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SolarPowerPlantsController> _logger;

        public SolarPowerPlantsController(ApplicationDbContext context, ILogger<SolarPowerPlantsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet("GetAll")]
        public async Task<ActionResult<IEnumerable<SolarPowerPlant>>> GetAll()
        {
            _logger.LogInformation("SolarPowerPlant/GetAll -> called at {Time}", DateTime.Now);
            return await _context.SolarPowerPlants.ToListAsync();
        }

        [HttpGet("Get")]
        public async Task<ActionResult<SolarPowerPlant>> Get(int id)
        {
            var plant = await _context.SolarPowerPlants.FindAsync(id);
            if (plant == null)
            {
                _logger.LogError("SolarPowerPlant/Get/{id} -> failed because a plant was not found", id);
                return NotFound();
            }

            _logger.LogInformation("SolarPowerPlant/Get/{id} -> succeeded", id);
            return plant;
        }

        [Authorize]
        [HttpPost("Create")]
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

            _logger.LogInformation("SolarPowerPlant/Create -> succeeded and made plant: {plant}", JsonSerializer.Serialize(plant));
            return CreatedAtAction(nameof(Get), new { id = plant.Id }, plant);
        }

        [Authorize]
        [HttpPut("Update")]
        public async Task<ActionResult<SolarPowerPlant>> Update(int id, SolarPowerPlantObject plantObject)
        {
            var plant = await _context.SolarPowerPlants.FindAsync(id);
            if (plant == null)
            {
                _logger.LogError("SolarPowerPlant/Update/{id} -> failed because a plant was not found", id);
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
                    _logger.LogError("SolarPowerPlant/Update/{id} -> failed because the plant stopped existing during update", id);
                    return NotFound();
                }
                else
                {
                    _logger.LogError("SolarPowerPlant/Update/{id} -> failed because rows were changed unexpectedly during update", id);
                    throw;
                }
            }

            _logger.LogInformation("SolarPowerPlant/Update/{id} -> succeeded and made plant: {plant}", id, JsonSerializer.Serialize(plant));
            return Ok();
        }

        [Authorize]
        [HttpDelete("Delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var plant = await _context.SolarPowerPlants.FindAsync(id);
            if (plant == null)
            {
                _logger.LogError("SolarPowerPlant/Delete/{id} -> failed because a plant was not found", id);
                return NotFound();
            }

            _context.SolarPowerPlants.Remove(plant);
            await _context.SaveChangesAsync();

            _logger.LogInformation("SolarPowerPlant/Update/{id} -> succeeded in deleting the plant", id);
            return Ok();
        }

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
