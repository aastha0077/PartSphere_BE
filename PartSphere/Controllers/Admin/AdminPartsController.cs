using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.DTOs;
using PartSphere.Services;

namespace PartSphere.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/parts")]
    [Authorize(Roles = "Admin")]
    public class AdminPartsController : ControllerBase
    {
        private readonly IPartService _partService;

        public AdminPartsController(IPartService partService)
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

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePartDto dto)
        {
            var part = await _partService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = part.Id }, part);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePartDto dto)
        {
            var part = await _partService.UpdateAsync(id, dto);
            return Ok(part);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _partService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStock()
        {
            var parts = await _partService.GetLowStockAsync();
            return Ok(parts);
        }
    }
}
