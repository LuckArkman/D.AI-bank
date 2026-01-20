using Fintech.Core.Entities;
using Fintech.Entities;

namespace Fintech.Core.Interfaces;

public interface IPixKeyRepository
{
    // Adiciona uma nova chave
    Task AddAsync(PixKey pixKey);

    // Busca uma chave específica (para saber de quem é a conta)
    Task<PixKey?> GetByKeyAsync(string key);

    // Verifica se a chave já existe (para validação antes de inserir)
    Task<bool> ExistsAsync(string key);

    // Busca todas as chaves de um usuário/conta
    Task<List<PixKey>> GetByAccountIdAsync(Guid accountId);
}