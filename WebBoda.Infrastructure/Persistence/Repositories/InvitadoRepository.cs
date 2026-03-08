using Microsoft.EntityFrameworkCore;
using WebBoda.Domain.Entities;
using WebBoda.Domain.Interfaces;
using WebBoda.Infrastructure.Persistence;

namespace WebBoda.Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementación concreta del IInvitadoRepository usando Entity Framework Core
/// con el proveedor de PostgreSQL (Npgsql).
///
/// Al implementar la interfaz definida en Domain, cumplimos el principio de
/// inversión de dependencias: las capas internas (Domain, Application) no saben
/// nada de EF Core. Solo Infrastructure conoce estos detalles técnicos.
/// </summary>
public class InvitadoRepository : IInvitadoRepository
{
    private readonly AppDbContext _context;

    public InvitadoRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// No usamos AsNoTracking() aquí porque ModificarFormularioHandler necesita
    /// que EF Core rastree la entidad en memoria para detectar los cambios
    /// y generar el UPDATE correcto al llamar a SaveChangesAsync.
    /// </summary>
    public async Task<Invitado?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.Invitados
            .FirstOrDefaultAsync(i => i.TokenAcceso == token, cancellationToken);
    }

    public async Task<IEnumerable<Invitado>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Invitados
            .AsNoTracking()
            .OrderBy(i => i.Apellidos)
            .ThenBy(i => i.Nombre)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// AddAsync encola el nuevo invitado en el contexto de EF Core.
    /// EF Core generará un INSERT en PostgreSQL cuando se llame a SaveChangesAsync.
    /// </summary>
    public async Task AddAsync(Invitado invitado, CancellationToken cancellationToken = default)
    {
        await _context.Invitados.AddAsync(invitado, cancellationToken);
    }

    public Task UpdateAsync(Invitado invitado, CancellationToken cancellationToken = default)
    {
        _context.Invitados.Update(invitado);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}