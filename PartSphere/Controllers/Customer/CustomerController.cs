using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.DTOs;
using PartSphere.Services;

namespace PartSphere.Controllers.Customer
{
    [ApiController]
    [Route("api/customer")]
    [Authorize(Roles = "Customer")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly IAppointmentService _appointmentService;
        private readonly IReviewService _reviewService;
        private readonly INotificationService _notificationService;
        private readonly IPartService _partService;

        public CustomerController(
            ICustomerService customerService,
            IAppointmentService appointmentService,
            IReviewService reviewService,
            INotificationService notificationService,
            IPartService partService)
        {
            _customerService = customerService;
            _appointmentService = appointmentService;
            _reviewService = reviewService;
            _notificationService = notificationService;
            _partService = partService;
        }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        // ===== PROFILE =====
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetUserId();
            var customer = await _customerService.GetByUserIdAsync(userId);
            if (customer == null) return NotFound("Customer profile not found.");
            return Ok(customer);
        }

        // ===== PARTS BROWSING =====
        [HttpGet("parts")]
        public async Task<IActionResult> BrowseParts()
        {
            var parts = await _partService.GetAllAsync();
            return Ok(parts);
        }

        [HttpGet("parts/search")]
        public async Task<IActionResult> SearchParts([FromQuery] string query)
        {
            var parts = await _partService.SearchAsync(query);
            return Ok(parts);
        }

        // ===== APPOINTMENTS =====
        [HttpGet("appointments")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userId = GetUserId();
            var customer = await _customerService.GetByUserIdAsync(userId);
            if (customer == null) return NotFound("Customer profile not found.");
            var appointments = await _appointmentService.GetByCustomerAsync(customer.Id);
            return Ok(appointments);
        }

        [HttpPost("appointments")]
        public async Task<IActionResult> BookAppointment([FromBody] CreateAppointmentDto dto)
        {
            var userId = GetUserId();
            var customer = await _customerService.GetByUserIdAsync(userId);
            if (customer == null) return NotFound("Customer profile not found.");
            dto.CustomerId = customer.Id;
            var appointment = await _appointmentService.CreateAsync(dto);
            return Ok(appointment);
        }

        // ===== REVIEWS =====
        [HttpGet("reviews")]
        public async Task<IActionResult> GetMyReviews()
        {
            var userId = GetUserId();
            var customer = await _customerService.GetByUserIdAsync(userId);
            if (customer == null) return NotFound("Customer profile not found.");
            var reviews = await _reviewService.GetByCustomerAsync(customer.Id);
            return Ok(reviews);
        }

        [HttpPost("reviews")]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDto dto)
        {
            var userId = GetUserId();
            var customer = await _customerService.GetByUserIdAsync(userId);
            if (customer == null) return NotFound("Customer profile not found.");
            dto.CustomerId = customer.Id;
            var review = await _reviewService.CreateAsync(dto);
            return Ok(review);
        }

        // ===== NOTIFICATIONS =====
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var userId = GetUserId();
            var notifications = await _notificationService.GetByUserIdAsync(userId);
            return Ok(notifications);
        }

        [HttpPut("notifications/{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Ok(new { message = "Notification marked as read." });
        }

        // ===== PURCHASE HISTORY =====
        [HttpGet("history")]
        public async Task<IActionResult> GetPurchaseHistory()
        {
            var userId = GetUserId();
            var customer = await _customerService.GetByUserIdAsync(userId);
            if (customer == null) return NotFound("Customer profile not found.");
            var history = await _customerService.GetHistoryAsync(customer.Id);
            return Ok(history);
        }
    }
}
