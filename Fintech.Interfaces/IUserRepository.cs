using Fintech.Core.Entities;

namespace Fintech.Core.Interfaces;

public interface IUserRepository
{
    // Adiciona um novo usuário (usado no cadastro)
    Task AddAsync(User user);

    // Busca usuário por e-mail (usado no login)
    Task<User?> GetByEmailAsync(string email);

    // Verifica se o e-mail já existe (para evitar duplicação no cadastro)
    Task<bool> ExistsByEmailAsync(string email);

    // Busca usuário pelo ID (usado para obter detalhes do perfil)
    Task<User?> GetByIdAsync(Guid id);

    // Atualiza dados do usuário (MFA, etc)
    Task UpdateAsync(User user);
}
