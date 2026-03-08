using WebBoda.Domain.Entities;

namespace WebBoda.Domain.Interfaces;

/// <summary>
/// Contrato que define las operaciones de persistencia para la entidad Invitado.
/// Al definir la interfaz en Domain, el núcleo del negocio no depende de ningún
/// detalle técnico de base de datos. La implementación real con EF Core + PostgreSQL
/// vive en Infrastructure.
/// </summary>
public interface IInvitadoRepository
{
    /// <summary>
    /// Busca un invitado por su token de acceso único.
    /// Devuelve null si el token no existe, lo que se usará para denegar el acceso
    /// a la vista de modificación.
    /// </summary>
    Task<Invitado?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Devuelve todos los invitados. Útil para un eventual panel de administración.
    /// </summary>
    Task<IEnumerable<Invitado>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste un nuevo invitado en la base de datos (primer envío del formulario).
    /// </summary>
    Task AddAsync(Invitado invitado, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marca un invitado existente como modificado para que EF Core genere el UPDATE.
    /// </summary>
    Task UpdateAsync(Invitado invitado, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persiste los cambios pendientes en la base de datos.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
