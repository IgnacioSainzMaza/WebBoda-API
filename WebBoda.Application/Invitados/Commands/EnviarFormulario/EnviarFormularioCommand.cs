using WebBoda.Domain.Enums;

namespace WebBoda.Application.Invitados.Commands.EnviarFormulario;

/// <summary>
/// Representa el primer envío del formulario por parte de un invitado.
///
/// Este command se ejecuta cuando alguien rellena el formulario por primera vez.
/// No existe ningún registro previo en la BD: el handler creará la entidad,
/// generará el token de acceso y enviará el email de confirmación al invitado.
/// </summary>
public class EnviarFormularioCommand
{
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;

    /// <summary>
    /// Email al que se enviará el resumen de respuestas y el enlace de modificación.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    public bool Asistencia { get; set; }
    public string? CondicionesAlimentarias { get; set; }
    public bool AutobusIda { get; set; }
    public TipoAutobusVuelta AutobusVuelta { get; set; }
    public string? Cancion { get; set; }
}