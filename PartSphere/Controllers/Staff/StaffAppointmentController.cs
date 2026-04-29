using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.DTOs;
using PartSphere.Services;

namespace PartSphere.Controllers.Staff
{
    [ApiController]
    [Route("api/staff/appointments")]
    [Authorize(Roles = "Admin,Staff")]
    public class StaffAppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public StaffAppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var appointments = await _appointmentService.GetAllAsync();
            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            return Ok(appointment);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateAppointmentStatusDto dto)
        {
            var appointment = await _appointmentService.UpdateStatusAsync(id, dto.Status);
            return Ok(appointment);
        }
    }
}
