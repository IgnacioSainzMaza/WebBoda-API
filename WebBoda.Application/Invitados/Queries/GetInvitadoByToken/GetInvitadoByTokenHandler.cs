using WebBoda.Application.DTOs;
using WebBoda.Application.Invitados.Queries.GetInvitadoByToken;
using WebBoda.Domain.Interfaces;

namespace WebBoda.Application.Invitados.Queries.GetInvitadoByToken;

/// <summary>
/// Contiene la lógica para ejecutar la consulta GetInvitadoByTokenQuery.
/// Delega la persistencia al repositorio y transforma el resultado en un DTO
/// apto para ser devuelto al frontend.
/// </summary>
public class GetInvitadoByTokenHandler
{
    private readonly IInvitadoRepository _invitadoRepository;

    public GetInvitadoByTokenHandler(IInvitadoRepository invitadoRepository)
    {
        _invitadoRepository = invitadoRepository;
    }

    /// <summary>
    /// Ejecuta la consulta. Si el token no existe devuelve null para que el
    /// controlador pueda responder con un 404 al frontend.
    /// </summary>
    public async Task<InvitadoDto?> HandleAsync(
        GetInvitadoByTokenQuery query,
        CancellationToken cancellationToken = default)
    {
        var invitado = await _invitadoRepository.GetByTokenAsync(query.Token, cancellationToken);

        if (invitado is null)
            return null;

        return new InvitadoDto
        {
            Nombre = invitado.Nombre,
            Apellidos = invitado.Apellidos,
            Email = invitado.Email,
            Asistencia = invitado.Asistencia,
            CondicionesAlimentarias = invitado.CondicionesAlimentarias,
            AutobusIda = invitado.AutobusIda,
            AutobusVuelta = invitado.AutobusVuelta,
            Cancion = invitado.Cancion
        };
    }
}