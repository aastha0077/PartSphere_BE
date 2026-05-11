using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.DTOs;
using PartSphere.Services;

namespace PartSphere.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartsController : ControllerBase
    {
        private readonly IPartService _partService;

        public PartsController(IPartService partService)
        {
            _partService = partService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search, 
            [FromQuery] string? category, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            var (items, total) = await _partService.GetAllAsync(search, category, page, pageSize);
            return Ok(new { items, total, page, pageSize });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var part = await _partService.GetByIdAsync(id);
            return Ok(part);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreatePartDto dto)
        {
            var part = await _partService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = part.Id }, part);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePartDto dto)
        {
            var part = await _partService.UpdateAsync(id, dto);
            return Ok(part);
        }

        [HttpPatch("{id}/stock")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto dto)
        {
            var part = await _partService.UpdateStockAsync(id, dto);
            return Ok(part);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            await _partService.DeleteAsync(id);
            return NoContent();
        }
    }
}
