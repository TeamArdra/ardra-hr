using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Review
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("reviewerRegNumber")]
    [Required]
    public string ReviewerRegNumber { get; set; } = string.Empty;

    [BsonElement("subjectRegNumber")]
    [Required]
    public string SubjectRegNumber { get; set; } = string.Empty;

    [BsonElement("content")]
    [Required]
    public string Content { get; set; } = string.Empty;

    [BsonElement("rating")]
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [BsonElement("month_year")]
    public string MonthYear { get; set; } = string.Empty;

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}