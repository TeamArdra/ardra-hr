using MongoDB.Driver;
using backend.DTOs;
using backend.Models;

namespace backend.Services;

public interface IUserService
{
    Task<User?> GetUserByRegNumberAsync(string regNumber);
    Task<User?> GetUserByEmailOrRegNumberAsync(string email, string regNumber);
    Task<User> CreateUserAsync(UserSignupDto userDto, string hashedPassword);
}

public class UserService : IUserService
{
    private readonly MongoDbService _mongoDb;

    public UserService(MongoDbService mongoDb)
    {
        _mongoDb = mongoDb;
    }

    public async Task<User?> GetUserByRegNumberAsync(string regNumber)
    {
        return await _mongoDb.Users
            .Find(u => u.RegNumber == regNumber)
            .FirstOrDefaultAsync();
    }

    public async Task<User?> GetUserByEmailOrRegNumberAsync(string email, string regNumber)
    {
        var filter = Builders<User>.Filter.Or(
            Builders<User>.Filter.Eq(u => u.VitEmail, email),
            Builders<User>.Filter.Eq(u => u.RegNumber, regNumber)
        );

        return await _mongoDb.Users.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<User> CreateUserAsync(UserSignupDto userDto, string hashedPassword)
    {
        var user = new User
        {
            Name = userDto.Name,
            RegNumber = userDto.RegNumber,
            Mobile = userDto.Mobile,
            VitEmail = userDto.VitEmail,
            PersonalEmail = userDto.PersonalEmail,
            TeamNumber = userDto.TeamNumber,
            Codename = userDto.Codename,
            Password = hashedPassword,
            ResidenceType = userDto.ResidenceType,
            HostelType = userDto.HostelType,
            BlockRoom = userDto.BlockRoom,
            CreatedAt = DateTime.UtcNow
        };

        await _mongoDb.Users.InsertOneAsync(user);
        return user;
    }
}