using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateReview([FromBody] ReviewCreateDto reviewDto)
    {
        try
        {
            var reviewId = await _reviewService.CreateReviewAsync(reviewDto);
            return StatusCode(StatusCodes.Status201Created, new { id = reviewId });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { detail = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReviewResponseDto>>> GetReviews(
        [FromQuery] string? subjectRegNumber)
    {
        if (string.IsNullOrEmpty(subjectRegNumber))
        {
            return Ok(new List<ReviewResponseDto>());
        }

        var reviews = await _reviewService.GetReviewsAsync(subjectRegNumber);
        return Ok(reviews);
    }

    [HttpGet("people")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PersonDto>>> GetPeople(
        [FromQuery] string? excludeRegNumber)
    {
        var people = await _reviewService.GetPeopleAsync(excludeRegNumber);
        return Ok(people);
    }
}