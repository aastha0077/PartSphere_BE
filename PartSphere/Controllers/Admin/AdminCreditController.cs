using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.DTOs;
using PartSphere.Services;

namespace PartSphere.Controllers.Admin
{
    [ApiController]
    [Route("api/admin/credits")]
    [Authorize(Roles = "Admin")]
    public class AdminCreditController : ControllerBase
    {
        private readonly ICreditService _creditService;

        public AdminCreditController(ICreditService creditService)
        {
            _creditService = creditService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var credits = await _creditService.GetAllAsync();
            return Ok(credits);
        }

        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetByCustomer(int customerId)
        {
            var credits = await _creditService.GetByCustomerAsync(customerId);
            return Ok(credits);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCreditPaymentDto dto)
        {
            var credit = await _creditService.CreateAsync(dto);
            return Ok(credit);
        }

        [HttpPut("{id}/paid")]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var credit = await _creditService.MarkAsPaidAsync(id);
            return Ok(credit);
        }

        [HttpPost("send-reminders")]
        public async Task<IActionResult> SendReminders()
        {
            await _creditService.SendOverdueRemindersAsync();
            return Ok(new { message = "Overdue reminders sent." });
        }
    }
}
