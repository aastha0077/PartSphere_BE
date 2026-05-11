using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.Models;
using PartSphere.Services;

namespace PartSphere.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;
        private readonly ICustomerService _customerService;

        public VehicleController(IVehicleService vehicleService, ICustomerService customerService)
        {
            _vehicleService = vehicleService;
            _customerService = customerService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        [HttpGet("my-vehicles")]
        public async Task<IActionResult> GetMyVehicles()
        {
            var customer = await _customerService.GetByUserIdAsync(GetUserId());
            if (customer == null) return NotFound("Customer profile not found");
            var vehicles = await _vehicleService.GetByCustomerIdAsync(customer.Id);
            return Ok(vehicles);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterVehicle([FromBody] PartSphere.DTOs.CreateVehicleDto vehicle)
        {
            var customer = await _customerService.GetByUserIdAsync(GetUserId());
            if (customer == null) return NotFound("Customer profile not found");
            vehicle.CustomerId = customer.Id;

            var result = await _vehicleService.CreateAsync(vehicle);
            return CreatedAtAction(nameof(GetMyVehicles), new { id = result.Id }, result);
        }

        [HttpPut("{id}/mileage")]
        public async Task<IActionResult> UpdateMileage(int id, [FromBody] int mileage)
        {
            await _vehicleService.UpdateAsync(id, new PartSphere.DTOs.UpdateVehicleDto { Mileage = mileage });
            return NoContent();
        }
    }
}
