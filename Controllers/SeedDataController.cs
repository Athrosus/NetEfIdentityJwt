﻿using IdentityJwtWeather.Data.Models;
using IdentityJwtWeather.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace IdentityJwtWeather.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedDataController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly Random _random = new Random();

        public SeedDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("SeedData")]
        public async Task<IActionResult> SeedData()
        {
            // Create 5 solar power plants
            for (int i = 0; i < 5; i++)
            {
                decimal installedPower = (decimal)( 5000 + 10000 * _random.NextDouble());
                var plant = new SolarPowerPlant
                {
                    Name = $"Plant{i}",
                    InstalledPower = installedPower,
                    DateOfInstallation = DateTime.Now.AddDays(-_random.Next(0, 365)),
                    Latitude = (decimal)(_random.NextDouble() * 180 - 90),
                    Longitude = (decimal)(_random.NextDouble() * 360 - 180)
                };
                _context.SolarPowerPlants.Add(plant);
            }
            await _context.SaveChangesAsync();

            // Generate production data for each plant for 5 days
            DateTime productionStart = DateTime.Now.AddDays(-5);
            DateTime productionEnd = DateTime.Now;
            var plants = await _context.SolarPowerPlants.ToListAsync();

            foreach (var plant in plants)
            {
                for (DateTime current = productionStart; current <= productionEnd; current = current.AddMinutes(15))
                {
                    decimal production = plant.InstalledPower * (decimal)_random.NextDouble();
                    var productionRecord = new SolarPowerPlantProduction
                    {
                        SolarPlantId = plant.Id,
                        Date = current,
                        Production = production
                    };
                    _context.SolarPowerPlantProduction.Add(productionRecord);
                }
            }
            await _context.SaveChangesAsync();

            return Ok("Seeded 5 solar plants and 5 days of production data.");
        }
    }
}
