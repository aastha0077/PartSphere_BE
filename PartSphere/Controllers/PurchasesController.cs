using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.DTOs;
using PartSphere.Services;

namespace PartSphere.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Staff")]
    public class PurchasesController : ControllerBase
    {
        private readonly IPurchaseService _purchaseService;

        public PurchasesController(IPurchaseService purchaseService)
        {
            _purchaseService = purchaseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? vendorId)
        {
            var purchases = await _purchaseService.GetAllAsync(vendorId);
            return Ok(purchases);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var purchase = await _purchaseService.GetByIdAsync(id);
            return Ok(purchase);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseInvoiceDto dto)
        {
            var purchase = await _purchaseService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = purchase.Id }, purchase);
        }
    }
}
