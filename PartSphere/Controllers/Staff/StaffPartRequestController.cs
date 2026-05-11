using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.Services;
using PartSphere.DTOs;

namespace PartSphere.Controllers.Staff
{
    [Authorize(Roles = "Admin,Staff")]
    [ApiController]
    [Route("api/staff/part-requests")]
    public class StaffPartRequestController : ControllerBase
    {
        private readonly IPartRequestService _partRequestService;

        public StaffPartRequestController(IPartRequestService partRequestService)
        {
            _partRequestService = partRequestService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _partRequestService.GetAllAsync();
            return Ok(requests);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdatePartRequestStatusDto dto)
        {
            try
            {
                var request = await _partRequestService.UpdateStatusAsync(id, dto.Status, dto.StaffNotes);
                return Ok(request);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }

    public class UpdatePartRequestStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string? StaffNotes { get; set; }
    }
}
