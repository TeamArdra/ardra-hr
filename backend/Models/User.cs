using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    [Required]
    public string Name { get; set; } = string.Empty;

    [BsonElement("regNumber")]
    [Required]
    public string RegNumber { get; set; } = string.Empty;

    [BsonElement("mobile")]
    [Required]
    public string Mobile { get; set; } = string.Empty;

    [BsonElement("vitEmail")]
    [Required]
    [EmailAddress]
    public string VitEmail { get; set; } = string.Empty;

    [BsonElement("personalEmail")]
    [Required]
    [EmailAddress]
    public string PersonalEmail { get; set; } = string.Empty;

    [BsonElement("teamNumber")]
    [Required]
    public string TeamNumber { get; set; } = string.Empty;

    [BsonElement("codename")]
    [Required]
    public string Codename { get; set; } = string.Empty;

    [BsonElement("password")]
    [Required]
    public string Password { get; set; } = string.Empty;

    [BsonElement("residenceType")]
    [Required]
    public string ResidenceType { get; set; } = string.Empty;

    [BsonElement("hostelType")]
    public string? HostelType { get; set; }

    [BsonElement("blockRoom")]
    public string? BlockRoom { get; set; }

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
}