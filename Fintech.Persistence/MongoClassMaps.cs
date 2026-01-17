using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Fintech.ValueObjects;

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
    }
}