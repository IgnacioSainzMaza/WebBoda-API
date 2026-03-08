using WebBoda.Application.Invitados.Commands.EnviarFormulario;
using WebBoda.Application.Invitados.Commands.ModificarFormulario;
using WebBoda.Application.Invitados.Queries.GetInvitadoByToken;
using Microsoft.AspNetCore.Mvc;
using static System.Net.Mime.MediaTypeNames;

namespace  WebBoda.API.Controllers;

/// <summary>
/// Controlador REST con tres endpoints que cubren el ciclo de vida completo del formulario:
///   - GET  /api/invitados/{token}  → Precarga el formulario con las respuestas anteriores.
///   - POST /api/invitados          → Primer envío del formulario (acceso libre, sin token).
///   - PUT  /api/invitados/{token}  → Modificación de un formulario ya enviado (requiere token).
///
/// Nótese que GET y PUT usan el token en la URL (estilo REST puro: el token identifica
/// el recurso), mientras que POST no lo usa porque el recurso aún no existe.
/// El middleware de validación de token solo se aplica a las rutas que lo incluyen.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InvitadosController : ControllerBase
{
    private readonly GetInvitadoByTokenHandler _getInvitadoHandler;
    private readonly EnviarFormularioHandler _enviarFormularioHandler;
    private readonly ModificarFormularioHandler _modificarFormularioHandler;

    public InvitadosController(
        GetInvitadoByTokenHandler getInvitadoHandler,
        EnviarFormularioHandler enviarFormularioHandler,
        ModificarFormularioHandler modificarFormularioHandler)
    {
        _getInvitadoHandler = getInvitadoHandler;
        _enviarFormularioHandler = enviarFormularioHandler;
        _modificarFormularioHandler = modificarFormularioHandler;
    }

    /// <summary>
    /// GET /api/invitados/{token}
    ///
    /// Devuelve las respuestas previas del invitado para precargar el formulario
    /// de modificación en el frontend. El middleware ya habrá validado el token
    /// antes de que la petición llegue aquí.
    /// </summary>
    [HttpGet("{token}")]
    [ProducesResponseType(typeof(Application.DTOs.InvitadoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetByToken(string token, CancellationToken cancellationToken)
    {
        var query = new GetInvitadoByTokenQuery { Token = token };
        var resultado = await _getInvitadoHandler.HandleAsync(query, cancellationToken);

        if (resultado is null)
            return Unauthorized(new { error = "Token no válido." });

        return Ok(resultado);
    }

    /// <summary>
    /// POST /api/invitados
    ///
    /// Primer envío del formulario. No requiere ningún tipo de autenticación:
    /// cualquier persona que acceda a la web puede enviar sus datos.
    /// El backend genera el token, crea el registro y envía el email de confirmación.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Enviar(
        [FromBody] EnviarFormularioCommand command,
        CancellationToken cancellationToken)
    {
        await _enviarFormularioHandler.HandleAsync(command, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// PUT /api/invitados/{token}
    ///
    /// Modificación de un formulario ya enviado. El token en la URL identifica
    /// al invitado y el middleware lo valida antes de que la petición llegue aquí.
    /// Usamos PUT (y no POST) porque estamos actualizando un recurso existente
    /// identificado de forma explícita por su token.
    /// </summary>
    [HttpPut("{token}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Modificar(
        string token,
        [FromBody] ModificarFormularioCommand command,
        CancellationToken cancellationToken)
    {
        command.Token = token;

        var exito = await _modificarFormularioHandler.HandleAsync(command, cancellationToken);

        if (!exito)
            return Unauthorized(new { error = "Token no válido." });

        return NoContent();
    }
}