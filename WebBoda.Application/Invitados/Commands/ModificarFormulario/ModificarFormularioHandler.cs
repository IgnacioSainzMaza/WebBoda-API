using WebBoda.Application.Interfaces;
using WebBoda.Application.Invitados.Commands.ModificarFormulario;
using WebBoda.Domain.Interfaces;

namespace WebBoda.Application.Invitados.Commands.ModificarFormulario;

/// <summary>
/// Orquesta el flujo de modificación de un formulario ya enviado:
///   1. Recupera al invitado por su token (ya validado por el middleware).
///   2. Actualiza sus campos con los nuevos valores.
///   3. Envía al invitado el email con el resumen actualizado y el mismo enlace de modificación.
///   4. Notifica a los organizadores de la modificación.
///
/// El token no cambia al modificar: el invitado conserva el mismo enlace
/// que recibió en su email original.
/// </summary>
public class ModificarFormularioHandler
{
    private readonly IInvitadoRepository _invitadoRepository;
    private readonly IEmailService _emailService;

    public ModificarFormularioHandler(
        IInvitadoRepository invitadoRepository,
        IEmailService emailService)
    {
        _invitadoRepository = invitadoRepository;
        _emailService = emailService;
    }

    /// <summary>
    /// Devuelve false si el token no existe en BD. En la práctica esto no debería
    /// ocurrir nunca porque el middleware ya lo habrá validado, pero lo mantenemos
    /// como salvaguarda defensiva.
    /// </summary>
    public async Task<bool> HandleAsync(
        ModificarFormularioCommand command,
        CancellationToken cancellationToken = default)
    {
        var invitado = await _invitadoRepository.GetByTokenAsync(command.Token, cancellationToken);

        if (invitado is null)
            return false;

        // Actualizamos únicamente los campos del formulario. Los datos de identidad
        // (Nombre, Apellidos, Email) y los metadatos de creación (FechaEnvio, TokenAcceso)
        // permanecen intactos: no tiene sentido que el invitado pueda cambiarlos.
        invitado.Asistencia = command.Asistencia;
        invitado.CondicionesAlimentarias = command.CondicionesAlimentarias;
        invitado.AutobusIda = command.AutobusIda;
        invitado.AutobusVuelta = command.AutobusVuelta;
        invitado.Cancion = command.Cancion;
        invitado.FechaUltimaModificacion = DateTime.UtcNow;

        await _invitadoRepository.UpdateAsync(invitado, cancellationToken);
        await _invitadoRepository.SaveChangesAsync(cancellationToken);

        await Task.WhenAll(
            _emailService.EnviarConfirmacionInvitadoAsync(invitado, cancellationToken),
            _emailService.EnviarNotificacionOrganizadoresAsync(
                $"{invitado.Nombre} {invitado.Apellidos}",
                esModificacion: true,
                cancellationToken)
        );

        return true;
    }
}