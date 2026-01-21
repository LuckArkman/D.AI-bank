using Fintech.Entities;
using Fintech.Interfaces;
using Fintech.Persistence;
using MongoDB.Driver;

namespace Fintech.Repositories;

public class RuleRepository : IRuleRepository
{
    private readonly MongoContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IMongoCollection<BusinessRule> _collection;

    public RuleRepository(MongoContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _collection = _context.Database.GetCollection<BusinessRule>("business_rules");
    }

    public async Task<BusinessRule> GetByIdAsync(string id)
    {
        var filter = Builders<BusinessRule>.Filter.And(
            Builders<BusinessRule>.Filter.Eq(x => x.Id, id),
            Builders<BusinessRule>.Filter.Eq(x => x.TenantId, _tenantProvider.TenantId)
        );

        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<List<BusinessRule>> GetAllAsync()
    {
        var filter = Builders<BusinessRule>.Filter.Eq(x => x.TenantId, _tenantProvider.TenantId);
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task AddAsync(BusinessRule rule)
    {
        rule.TenantId = _tenantProvider.TenantId ?? throw new Exception("TenantId is required");

        if (_context.Session != null)
        {
            await _collection.InsertOneAsync(_context.Session, rule);
        }
        else
        {
            await _collection.InsertOneAsync(rule);
        }
    }

    public async Task UpdateAsync(BusinessRule rule)
    {
        var filter = Builders<BusinessRule>.Filter.And(
            Builders<BusinessRule>.Filter.Eq(x => x.Id, rule.Id),
            Builders<BusinessRule>.Filter.Eq(x => x.TenantId, _tenantProvider.TenantId)
        );

        var update = Builders<BusinessRule>.Update
            .Set(x => x.Name, rule.Name)
            .Set(x => x.Description, rule.Description)
            .Set(x => x.ConditionExpression, rule.ConditionExpression)
            .Set(x => x.ErrorMessage, rule.ErrorMessage)
            .Set(x => x.Severity, rule.Severity);

        if (_context.Session != null)
        {
            await _collection.UpdateOneAsync(_context.Session, filter, update);
        }
        else
        {
            await _collection.UpdateOneAsync(filter, update);
        }
    }

    public async Task DeleteAsync(string id)
    {
        var filter = Builders<BusinessRule>.Filter.And(
            Builders<BusinessRule>.Filter.Eq(x => x.Id, id),
            Builders<BusinessRule>.Filter.Eq(x => x.TenantId, _tenantProvider.TenantId)
        );

        if (_context.Session != null)
        {
            await _collection.DeleteOneAsync(_context.Session, filter);
        }
        else
        {
            await _collection.DeleteOneAsync(filter);
        }
    }
}
