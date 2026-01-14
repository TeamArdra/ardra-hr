using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class ReviewCreateDto
{
    [Required]
    public string ReviewerRegNumber { get; set; } = string.Empty;

    [Required]
    public string SubjectRegNumber { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }
}

public class ReviewResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string ReviewerRegNumber { get; set; } = string.Empty;
    public string SubjectRegNumber { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string MonthYear { get; set; } = string.Empty;
    public string? CreatedAt { get; set; }
}

public class PersonDto
{
    public string Name { get; set; } = string.Empty;
    public string RegNumber { get; set; } = string.Empty;
}