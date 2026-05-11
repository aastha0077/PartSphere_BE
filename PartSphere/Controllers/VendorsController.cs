using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.DTOs;
using PartSphere.Services;

namespace PartSphere.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")]
    public class VendorsController : ControllerBase
    {
        private readonly IVendorService _vendorService;

        public VendorsController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var vendors = await _vendorService.GetAllAsync();
            return Ok(vendors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var vendor = await _vendorService.GetByIdAsync(id);
            return Ok(vendor);
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateVendorDto dto)
        {
            var vendor = await _vendorService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = vendor.Id }, vendor);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateVendorDto dto)
        {
            var vendor = await _vendorService.UpdateAsync(id, dto);
            return Ok(vendor);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            await _vendorService.DeleteAsync(id);
            return NoContent();
        }
    }
}
