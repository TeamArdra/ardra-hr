using MongoDB.Driver;
using backend.DTOs;
using backend.Models;

namespace backend.Services;

public interface IReviewService
{
    Task<string> CreateReviewAsync(ReviewCreateDto reviewDto);
    Task<List<ReviewResponseDto>> GetReviewsAsync(string subjectRegNumber);
    Task<List<PersonDto>> GetPeopleAsync(string? excludeRegNumber);
    Task CleanupOldReviewsAsync();
}

public class ReviewService : IReviewService
{
    private readonly MongoDbService _mongoDb;

    public ReviewService(MongoDbService mongoDb)
    {
        _mongoDb = mongoDb;
    }

    public async Task<string> CreateReviewAsync(ReviewCreateDto reviewDto)
    {
        // Verify reviewer and subject exist
        var reviewer = await _mongoDb.Users
            .Find(u => u.RegNumber == reviewDto.ReviewerRegNumber)
            .FirstOrDefaultAsync();
        
        var subject = await _mongoDb.Users
            .Find(u => u.RegNumber == reviewDto.SubjectRegNumber)
            .FirstOrDefaultAsync();

        if (reviewer == null || subject == null)
        {
            throw new InvalidOperationException("Reviewer or subject not found");
        }

        var review = new Review
        {
            ReviewerRegNumber = reviewDto.ReviewerRegNumber,
            SubjectRegNumber = reviewDto.SubjectRegNumber,
            Content = reviewDto.Content,
            Rating = reviewDto.Rating,
            MonthYear = GetCurrentMonthYear(),
            CreatedAt = DateTime.UtcNow
        };

        await _mongoDb.Reviews.InsertOneAsync(review);
        return review.Id!;
    }

    public async Task<List<ReviewResponseDto>> GetReviewsAsync(string subjectRegNumber)
    {
        var currentMonthYear = GetCurrentMonthYear();
        var reviews = await _mongoDb.Reviews
            .Find(r => r.SubjectRegNumber == subjectRegNumber && r.MonthYear == currentMonthYear)
            .ToListAsync();

        return reviews.Select(r => new ReviewResponseDto
        {
            Id = r.Id!,
            ReviewerRegNumber = r.ReviewerRegNumber,
            SubjectRegNumber = r.SubjectRegNumber,
            Content = r.Content,
            Rating = r.Rating,
            MonthYear = r.MonthYear,
            CreatedAt = r.CreatedAt.ToString("o")
        }).ToList();
    }

    public async Task<List<PersonDto>> GetPeopleAsync(string? excludeRegNumber)
    {
        var filter = string.IsNullOrEmpty(excludeRegNumber)
            ? Builders<User>.Filter.Empty
            : Builders<User>.Filter.Ne(u => u.RegNumber, excludeRegNumber);

        var projection = Builders<User>.Projection
            .Include(u => u.Name)
            .Include(u => u.RegNumber)
            .Exclude(u => u.Id);

        var users = await _mongoDb.Users
            .Find(filter)
            .Project(projection)
            .ToListAsync();

        return users.Select(u => new PersonDto
        {
            Name = u["name"].AsString,
            RegNumber = u["regNumber"].AsString
        }).ToList();
    }

    public async Task CleanupOldReviewsAsync()
    {
        var currentMonthYear = GetCurrentMonthYear();
        var filter = Builders<Review>.Filter.Ne(r => r.MonthYear, currentMonthYear);
        await _mongoDb.Reviews.DeleteManyAsync(filter);
        Console.WriteLine($"Cleaned up old reviews. Current month: {currentMonthYear}");
    }

    private string GetCurrentMonthYear()
    {
        var now = DateTime.UtcNow;
        return $"{now.Year:D4}-{now.Month:D2}";
    }
}