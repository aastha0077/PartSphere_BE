using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.DTOs;
using PartSphere.Services;

namespace PartSphere.Controllers.Staff
{
    [ApiController]
    [Route("api/staff/vehicles")]
    [Authorize(Roles = "Admin,Staff")]
    public class StaffVehicleController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public StaffVehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vehicles = await _vehicleService.GetAllAsync();
            return Ok(vehicles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vehicle = await _vehicleService.GetByIdAsync(id);
            return Ok(vehicle);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var vehicles = await _vehicleService.SearchAsync(query);
            return Ok(vehicles);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVehicleDto dto)
        {
            var vehicle = await _vehicleService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = vehicle.Id }, vehicle);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVehicleDto dto)
        {
            var vehicle = await _vehicleService.UpdateAsync(id, dto);
            return Ok(vehicle);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _vehicleService.DeleteAsync(id);
            return NoContent();
        }
    }
}
