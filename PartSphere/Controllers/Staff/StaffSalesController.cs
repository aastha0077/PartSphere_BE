using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.DTOs;
using PartSphere.Services;

namespace PartSphere.Controllers.Staff
{
    [ApiController]
    [Route("api/staff/sales")]
    [Authorize(Roles = "Admin,Staff")]
    public class StaffSalesController : ControllerBase
    {
        private readonly ISalesService _salesService;

        public StaffSalesController(ISalesService salesService)
        {
            _salesService = salesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var sales = await _salesService.GetAllAsync();
            return Ok(sales);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var sale = await _salesService.GetByIdAsync(id);
            return Ok(sale);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateSalesInvoiceDto dto)
        {
            var staffId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var sale = await _salesService.CreateAsync(staffId, dto);
            return CreatedAtAction(nameof(GetById), new { id = sale.Id }, sale);
        }
    }
}
