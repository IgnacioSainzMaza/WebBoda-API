using WebBoda.Domain.Enums;

namespace WebBoda.Application.Invitados.Commands.ModificarFormulario;

/// <summary>
/// Representa la modificación de un formulario ya enviado previamente.
///
/// A diferencia de EnviarFormularioCommand, aquí el invitado ya existe en BD
/// y se identifica mediante su token de acceso (que el middleware habrá validado
/// antes de que la petición llegue al controlador).
///
/// El email no es un campo modificable de forma deliberada: si el invitado
/// cambia su email, el enlace de modificación que tiene en su bandeja de entrada
/// seguirá funcionando correctamente con independencia del email almacenado.
/// </summary>
public class ModificarFormularioCommand
{
    /// <summary>
    /// Token del invitado que está modificando su formulario.
    /// El controlador lo inyecta desde HttpContext.Items tras la validación del middleware.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    public bool Asistencia { get; set; }
    public string? CondicionesAlimentarias { get; set; }
    public bool AutobusIda { get; set; }
    public TipoAutobusVuelta AutobusVuelta { get; set; }
    public string? Cancion { get; set; }
}