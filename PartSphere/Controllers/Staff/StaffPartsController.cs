using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.DTOs;
using PartSphere.Services;

namespace PartSphere.Controllers.Staff
{
    [ApiController]
    [Route("api/staff/parts")]
    [Authorize(Roles = "Admin,Staff")]
    public class StaffPartsController : ControllerBase
    {
        private readonly IPartService _partService;

        public StaffPartsController(IPartService partService)
        {
            _partService = partService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var parts = await _partService.GetAllAsync();
            return Ok(parts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var part = await _partService.GetByIdAsync(id);
            return Ok(part);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            var parts = await _partService.SearchAsync(query);
            return Ok(parts);
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock()
        {
            var parts = await _partService.GetLowStockAsync();
            return Ok(parts);
        }
    }
}
