using Microsoft.EntityFrameworkCore;
using PartSphere.DTOs;
using PartSphere.Models;
using PartSphere.Repositories;

namespace PartSphere.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<ReviewDto>> GetAllAsync();
        Task<IEnumerable<ReviewDto>> GetByCustomerAsync(int customerId);
        Task<ReviewDto> CreateAsync(CreateReviewDto dto);
    }

    public class ReviewService : IReviewService
    {
        private readonly IRepository<Review> _reviewRepo;
        private readonly IRepository<Customer> _customerRepo;

        public ReviewService(IRepository<Review> reviewRepo, IRepository<Customer> customerRepo)
        {
            _reviewRepo = reviewRepo;
            _customerRepo = customerRepo;
        }

        public async Task<IEnumerable<ReviewDto>> GetAllAsync()
        {
            List<Review> reviews = await _reviewRepo.Query()
                .Include(r => r.Customer)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Select(MapToDto);
        }

        public async Task<IEnumerable<ReviewDto>> GetByCustomerAsync(int customerId)
        {
            List<Review> reviews = await _reviewRepo.Query()
                .Include(r => r.Customer)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reviews.Select(MapToDto);
        }

        public async Task<ReviewDto> CreateAsync(CreateReviewDto dto)
        {
            if (!await _customerRepo.ExistsAsync(dto.CustomerId))
                throw new KeyNotFoundException("Customer not found.");

            Review review = new Review
            {
                CustomerId = dto.CustomerId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            await _reviewRepo.AddAsync(review);

            Review created = await _reviewRepo.Query()
                .Include(r => r.Customer)
                .FirstAsync(r => r.Id == review.Id);

            return MapToDto(created);
        }

        private static ReviewDto MapToDto(Review r) => new ReviewDto
        {
            Id = r.Id,
            CustomerId = r.CustomerId,
            CustomerName = r.Customer?.Name ?? "",
            Rating = r.Rating,
            Comment = r.Comment,
            CreatedAt = r.CreatedAt
        };
    }
}
