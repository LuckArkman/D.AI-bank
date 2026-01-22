using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Bson.Serialization.IdGenerators;
using Fintech.ValueObjects;
using Fintech.Entities;

namespace Fintech.Persistence;

public static class MongoClassMaps
{
    public static void Register()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(Money)))
        {
            BsonClassMap.RegisterClassMap<Money>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(m => m.Amount).SetSerializer(new DecimalSerializer(BsonType.Decimal128));
            });
        }

        if (!BsonClassMap.IsClassMapRegistered(typeof(Account)))
        {
            BsonClassMap.RegisterClassMap<Account>(cm =>
            {
                cm.AutoMap();
                cm.MapIdMember(c => c.Id).SetIdGenerator(GuidGenerator.Instance);
                cm.MapMember(c => c.TenantId).SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
            });
        }
    }
}