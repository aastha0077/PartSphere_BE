using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PartSphere.Services;

namespace PartSphere.Controllers
{
    [ApiController]
    [Route("api/invoices")]
    [Authorize]
    public class InvoiceController : ControllerBase
    {
        private readonly ISalesService _salesService;
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(ISalesService salesService, IInvoiceService invoiceService)
        {
            _salesService = salesService;
            _invoiceService = invoiceService;
        }

        [HttpGet("{orderId}/pdf")]
        public async Task<IActionResult> GetInvoicePdf(int orderId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = User.FindFirstValue(ClaimTypes.Role);

            var sale = await _salesService.GetModelByIdAsync(orderId);
            if (sale == null) return NotFound();

            if (role == "Customer" && sale.Customer.UserId != userId)
            {
                return Forbid();
            }

            var pdfBytes = _invoiceService.GenerateSalesInvoicePdf(sale);
            return File(pdfBytes, "application/pdf", $"Invoice_{sale.Invoice?.InvoiceNumber ?? sale.Id.ToString()}.pdf");
        }
    }
}
