using WebBoda.Domain.Enums;

namespace WebBoda.Application.DTOs;

/// <summary>
/// Objeto de transferencia de datos que representa la información del invitado
/// que se devuelve al frontend en el endpoint GET /api/invitados/{token}.
///
/// Usamos un DTO en lugar de exponer la entidad directamente por varios motivos:
/// evitamos exponer campos internos como el Id numérico de la BD, podemos dar
/// forma a los datos según lo que necesita el frontend, y si en el futuro cambia
/// la entidad, el DTO puede absorber ese cambio sin romper el contrato con el cliente.
/// </summary>
public class InvitadoDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Asistencia { get; set; }
    public string? CondicionesAlimentarias { get; set; }
    public bool AutobusIda { get; set; }
    public TipoAutobusVuelta AutobusVuelta { get; set; }
    public string? Cancion { get; set; }
}