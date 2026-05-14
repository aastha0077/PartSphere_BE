using Microsoft.EntityFrameworkCore;
using PartSphere.DTOs;
using PartSphere.Helpers;
using PartSphere.Models;
using PartSphere.Repositories;

namespace PartSphere.Services
{
    public interface ICreditService
    {
        Task<IEnumerable<CreditPaymentDto>> GetAllAsync();
        Task<IEnumerable<CreditPaymentDto>> GetByCustomerAsync(int customerId);
        Task<CreditPaymentDto> CreateAsync(CreateCreditPaymentDto dto);
        Task<CreditPaymentDto> MarkAsPaidAsync(int id);
        Task SendOverdueRemindersAsync();
    }

    public class CreditService : ICreditService
    {
        private readonly IRepository<CreditPayment> _creditRepo;
        private readonly IRepository<Customer> _customerRepo;
        private readonly IEmailService _emailService;
        private readonly ILogger<CreditService> _logger;
        private readonly IConfiguration _configuration;

        public CreditService(
            IRepository<CreditPayment> creditRepo,
            IRepository<Customer> customerRepo,
            IEmailService emailService,
            ILogger<CreditService> logger,
            IConfiguration configuration)
        {
            _creditRepo = creditRepo;
            _customerRepo = customerRepo;
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IEnumerable<CreditPaymentDto>> GetAllAsync()
        {
            var credits = await _creditRepo.Query()
                .Include(c => c.Customer)
                .Include(c => c.SalesInvoice)
                .OrderByDescending(c => c.DueDate)
                .ToListAsync();

            return credits.Select(MapToDto);
        }

        public async Task<IEnumerable<CreditPaymentDto>> GetByCustomerAsync(int customerId)
        {
            var credits = await _creditRepo.Query()
                .Include(c => c.Customer)
                .Include(c => c.SalesInvoice)
                .Where(c => c.CustomerId == customerId)
                .OrderByDescending(c => c.DueDate)
                .ToListAsync();

            return credits.Select(MapToDto);
        }

        public async Task<CreditPaymentDto> CreateAsync(CreateCreditPaymentDto dto)
        {
            var customer = await _customerRepo.GetByIdAsync(dto.CustomerId)
                ?? throw new KeyNotFoundException("Customer not found.");

            var credit = new CreditPayment
            {
                CustomerId = dto.CustomerId,
                SalesInvoiceId = dto.SalesInvoiceId,
                DueAmount = dto.DueAmount,
                DueDate = dto.DueDate,
                Status = CreditStatus.Pending
            };

            await _creditRepo.AddAsync(credit);
            _logger.LogInformation("Credit created for Customer {CustomerId}, Amount: {Amount}", dto.CustomerId, dto.DueAmount);

            return await GetByIdAsync(credit.Id);
        }

        public async Task<CreditPaymentDto> MarkAsPaidAsync(int id)
        {
            var credit = await _creditRepo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Credit payment not found.");

            credit.Status = CreditStatus.Paid;
            credit.PaidDate = DateTime.UtcNow;
            await _creditRepo.UpdateAsync(credit);

            return await GetByIdAsync(credit.Id);
        }

        public async Task SendOverdueRemindersAsync()
        {
            var minDaysPastDue = int.TryParse(_configuration["Alerts:CreditReminderMinDaysPastDue"], out var mpd) ? mpd : 30;
            var cooldownDays = int.TryParse(_configuration["Alerts:CreditReminderCooldownDays"], out var cd) ? cd : 14;
            var now = DateTime.UtcNow;
            var emailEligibleBefore = now.AddDays(-minDaysPastDue);
            var cooldownCutoff = now.AddDays(-cooldownDays);

            var overdue = await _creditRepo.Query()
                .Include(c => c.Customer)
                    .ThenInclude(c => c.User)
                .Where(c => c.Status != CreditStatus.Paid && c.DueDate < now)
                .ToListAsync();

            foreach (var credit in overdue)
            {
                if (credit.Status == CreditStatus.Pending)
                {
                    credit.Status = CreditStatus.Overdue;
                    await _creditRepo.UpdateAsync(credit);
                }

                if (credit.DueDate > emailEligibleBefore)
                    continue;

                if (credit.LastCreditReminderSentAt.HasValue && credit.LastCreditReminderSentAt.Value >= cooldownCutoff)
                    continue;

                var email = credit.Customer?.User?.Email ?? credit.Customer?.Email;
                if (!string.IsNullOrWhiteSpace(email))
                {
                    await _emailService.SendCreditReminderAsync(
                        email,
                        credit.Customer!.Name,
                        credit.DueAmount,
                        credit.DueDate);

                    credit.LastCreditReminderSentAt = now;
                    await _creditRepo.UpdateAsync(credit);
                }
            }

            _logger.LogInformation("Processed overdue credits: {Count} past due; emails only when due before {Threshold} (UTC) with {Cooldown}d cooldown.",
                overdue.Count, emailEligibleBefore, cooldownDays);
        }

        private async Task<CreditPaymentDto> GetByIdAsync(int id)
        {
            var credit = await _creditRepo.Query()
                .Include(c => c.Customer)
                .Include(c => c.SalesInvoice)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new KeyNotFoundException("Credit payment not found.");

            return MapToDto(credit);
        }

        private static CreditPaymentDto MapToDto(CreditPayment c) => new()
        {
            Id = c.Id,
            CustomerId = c.CustomerId,
            CustomerName = c.Customer?.Name ?? "",
            SalesInvoiceId = c.SalesInvoiceId,
            DueAmount = c.DueAmount,
            DueDate = c.DueDate,
            PaidDate = c.PaidDate,
            Status = c.Status.ToString()
        };
    }
}
