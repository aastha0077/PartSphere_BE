using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.DTOs;
using PartSphere.Services;
using PartSphere.Models;
using PartSphere.Data;
using Microsoft.EntityFrameworkCore;

namespace PartSphere.Controllers
{
    [ApiController]
    [Route("api/orders")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly ISalesService _salesService;
        private readonly ICustomerService _customerService;
        private readonly AppDbContext _context;

        public OrderController(ISalesService salesService, ICustomerService customerService, AppDbContext context)
        {
            _salesService = salesService;
            _customerService = customerService;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);

            IEnumerable<SalesInvoiceDto> orders = await _salesService.GetAllAsync();

            if (role == "Customer")
            {
                var customer = await _customerService.GetByUserIdAsync(userId);
                if (customer == null)
                    return Ok(new List<SalesInvoiceDto>());

                orders = orders.Where(o => o.CustomerId == customer.Id);
            }

            var sorted = orders
                .OrderByDescending(o => o.Date)
                .ThenByDescending(o => o.Id);

            return Ok(sorted.ToList());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);

            var order = await _salesService.GetByIdAsync(id);

            if (role == "Customer")
            {
                var customer = await _customerService.GetByUserIdAsync(userId);
                if (customer == null)
                    return NotFound("Customer profile not found.");

                if (order.CustomerId != customer.Id)
                    return Forbid();
            }

            return Ok(order);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> Create([FromBody] CreateSalesInvoiceDto dto)
        {
            var staffId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var sale = await _salesService.CreateAsync(staffId, dto);
            return CreatedAtAction(nameof(GetById), new { id = sale.Id }, sale);
        }

        [HttpPost("checkout")]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CustomerCheckout([FromBody] CreateSalesInvoiceDto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var customer = await _customerService.GetByUserIdAsync(userId);
            if (customer == null) return NotFound("Customer profile not found.");

            dto.CustomerId = customer.Id;
            dto.PaymentStatus = "Pending";
            if (string.IsNullOrEmpty(dto.PaymentMethod))
            {
                dto.PaymentMethod = "Online";
            }

            var admin = await _context.Users.FirstOrDefaultAsync(u => u.Role == UserRole.Admin || u.Role == UserRole.Staff);
            int processingStaffId = admin?.Id ?? 1;

            var sale = await _salesService.CreateAsync(processingStaffId, dto);
            return Ok(sale);
        }

        [HttpPost("{id}/email")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> SendEmail(int id)
        {
            try
            {
                await _salesService.SendInvoiceEmailAsync(id);
                return Ok(new { message = "Invoice email sent successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}/complete")]
        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> CompleteOrder(int id)
        {
            try
            {
                var sale = await _salesService.CompleteOrderAsync(id);
                return Ok(sale);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
