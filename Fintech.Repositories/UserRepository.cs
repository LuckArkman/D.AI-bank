using MongoDB.Driver;
using Fintech.Core.Entities;
using Fintech.Core.Interfaces;
using Fintech.Interfaces;
using Fintech.Persistence;

namespace Fintech.Repositories;

public class UserRepository : IUserRepository
{
    private readonly MongoContext _context;
    private readonly IMongoCollection<User> _collection;
    private readonly ITenantProvider _tenantProvider;

    public UserRepository(MongoContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
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
        return await _collection.Find(u => u.Email == email && u.TenantId == _tenantProvider.TenantId).FirstOrDefaultAsync();
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _collection.Find(u => u.Email == email && u.TenantId == _tenantProvider.TenantId).AnyAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _collection.Find(u => u.Id == id && u.TenantId == _tenantProvider.TenantId).FirstOrDefaultAsync();
    }

    public async Task UpdateAsync(User user)
    {
        if (_context.Session != null)
            await _collection.ReplaceOneAsync(_context.Session, u => u.Id == user.Id && u.TenantId == _tenantProvider.TenantId, user);
        else
            await _collection.ReplaceOneAsync(u => u.Id == user.Id && u.TenantId == _tenantProvider.TenantId, user);
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _collection.Find(u => u.Email == email && u.TenantId == _tenantProvider.TenantId).AnyAsync();
    }
}