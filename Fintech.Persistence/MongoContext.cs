using MongoDB.Driver;
using Fintech.Interfaces;

namespace Fintech.Persistence;

public class MongoContext : ITransactionManager
{
    private readonly IMongoClient _client;
    private IClientSessionHandle? _currentSession;

    public MongoContext(IMongoClient client) => _client = client;

    public IMongoDatabase Database => _client.GetDatabase("FintechDB");
    public IClientSessionHandle? Session => _currentSession;

    public async Task<IUnitOfWork> BeginTransactionAsync()
    {
        if (_currentSession != null) throw new InvalidOperationException("Transação já iniciada.");

        _currentSession = await _client.StartSessionAsync();
        bool isTransactionStarted = false;

        try
        {
            _currentSession.StartTransaction(new TransactionOptions(
                readConcern: ReadConcern.Snapshot,
                writeConcern: WriteConcern.WMajority));
            isTransactionStarted = true;
        }
        catch (NotSupportedException)
        {
            // Fallback: Servidores Standalone do MongoDB não suportam transações multi-documento.
            // Permitimos que a operação continue sem transação para facilitar o desenvolvimento local.
        }

        return new MongoUnitOfWork(_currentSession, isTransactionStarted, this);
    }

    private class MongoUnitOfWork : IUnitOfWork
    {
        private readonly IClientSessionHandle _session;
        private readonly bool _hasActiveTransaction;
        private readonly MongoContext _context;

        public MongoUnitOfWork(IClientSessionHandle session, bool hasActiveTransaction, MongoContext context)
        {
            _session = session;
            _hasActiveTransaction = hasActiveTransaction;
            _context = context;
        }

        public async Task CommitAsync(CancellationToken ct)
        {
            if (_hasActiveTransaction && _session.IsInTransaction)
            {
                await _session.CommitTransactionAsync(ct);
            }
            Cleanup();
        }

        public async Task AbortAsync(CancellationToken ct)
        {
            if (_hasActiveTransaction && _session.IsInTransaction)
            {
                await _session.AbortTransactionAsync(ct);
            }
            Cleanup();
        }

        public void Dispose() => Cleanup();

        private void Cleanup()
        {
            if (_context._currentSession == _session)
            {
                _context._currentSession = null;
            }
            _session?.Dispose();
        }
    }
}