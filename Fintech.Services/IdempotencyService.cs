using Fintech.Entities;
using MongoDB.Driver;

namespace Fintech.Services;

public class IdempotencyService
{
    private readonly IMongoCollection<IdempotencyRecord> _records;

    public IdempotencyService(IMongoClient client)
    {
        var db = client.GetDatabase("FintechDB");
        _records = db.GetCollection<IdempotencyRecord>("idempotency_logs");
    }

    // Tenta reservar a chave. Se já existe, retorna o registro anterior.
    public async Task<IdempotencyRecord?> TryLockAsync(Guid commandId, IClientSessionHandle session)
    {
        try
        {
            var record = new IdempotencyRecord
            {
                Id = commandId,
                ProcessedAt = DateTime.UtcNow,
                Success = false // Ainda processando
            };
            
            // Tenta inserir. Se falhar por chave duplicada, é retry.
            await _records.InsertOneAsync(session, record);
            return null; // Null significa "Pode prosseguir, é novo"
        }
        catch (MongoWriteException ex) when (ex.WriteError.Category == ServerErrorCategory.DuplicateKey)
        {
            // Já existe! Retorna o resultado salvo anteriormente.
            return await _records.Find(session, x => x.Id == commandId).FirstOrDefaultAsync();
        }
    }

    public async Task CompleteAsync(Guid commandId, bool success, object result, IClientSessionHandle session)
    {
        var update = Builders<IdempotencyRecord>.Update
            .Set(x => x.Success, success)
            .Set(x => x.ResultJson, System.Text.Json.JsonSerializer.Serialize(result));

        await _records.UpdateOneAsync(session, x => x.Id == commandId, update);
    }
}