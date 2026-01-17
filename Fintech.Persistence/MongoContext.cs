using MongoDB.Driver;
using Fintech.Interfaces;

namespace Fintech.Persistence;

public class MongoContext : ITransactionManager
{
    private readonly IMongoClient _client;
    private IClientSessionHandle _currentSession;

    public MongoContext(IMongoClient client) => _client = client;

    public IMongoDatabase Database => _client.GetDatabase("FintechDB");
    public IClientSessionHandle? Session => _currentSession;

    public async Task<IUnitOfWork> BeginTransactionAsync()
    {
        if (_currentSession != null) throw new InvalidOperationException("Transação já iniciada.");
        
        _currentSession = await _client.StartSessionAsync();
        _currentSession.StartTransaction(new TransactionOptions(
            readConcern: ReadConcern.Snapshot, 
            writeConcern: WriteConcern.WMajority));

        return new MongoUnitOfWork(_currentSession);
    }

    private class MongoUnitOfWork : IUnitOfWork
    {
        private readonly IClientSessionHandle _session;
        public MongoUnitOfWork(IClientSessionHandle session) => _session = session;
        
        public async Task CommitAsync(CancellationToken ct) { await _session.CommitTransactionAsync(ct); _session.Dispose(); }
        public async Task AbortAsync(CancellationToken ct) { await _session.AbortTransactionAsync(ct); _session.Dispose(); }
        public void Dispose() => _session?.Dispose();
    }
}