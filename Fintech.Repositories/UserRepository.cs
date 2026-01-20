using MongoDB.Driver;
using Fintech.Core.Entities;
using Fintech.Core.Interfaces;
using Fintech.Persistence;

namespace Fintech.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MongoContext _context;
    private readonly IMongoCollection<User> _collection;

    public UserRepository(MongoContext context)
    {
        _context = context;
        _collection = _context.Database.GetCollection<User>("users");
    }

    public async Task AddAsync(User user)
    {
        if (_context.Session != null)
            await _collection.InsertOneAsync(_context.Session, user);
        else
            await _collection.InsertOneAsync(user);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _collection.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _collection.Find(u => u.Email == email).AnyAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _collection.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(User user)
    {
        if (_context.Session != null)
            await _collection.ReplaceOneAsync(_context.Session, u => u.Id == user.Id, user);
        else
            await _collection.ReplaceOneAsync(u => u.Id == user.Id, user);
    }

    public async Task<bool> ExistsAsync(string email)

    {
        return await _collection.Find(u => u.Email == email).AnyAsync();
    }
}