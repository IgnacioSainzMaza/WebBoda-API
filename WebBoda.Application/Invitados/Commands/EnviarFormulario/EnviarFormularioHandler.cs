using WebBoda.Application.Interfaces;
using WebBoda.Application.Invitados.Commands.EnviarFormulario;
using WebBoda.Domain.Entities;
using WebBoda.Domain.Interfaces;

namespace WebBoda.Application.Invitados.Commands.EnviarFormulario;

/// <summary>
/// Orquesta el flujo completo del primer envío del formulario:
///   1. Genera un token único y seguro para este invitado.
///   2. Crea la entidad Invitado con todos sus datos y la persiste en BD.
///   3. Envía al invitado el email con el resumen de respuestas y el enlace de modificación.
///   4. Notifica a los organizadores del nuevo registro.
///
/// La generación del token se realiza aquí en Application y no en la entidad
/// de Domain de forma deliberada: Domain no debe conocer detalles técnicos
/// como Guid. El handler actúa como orquestador y es el lugar correcto
/// para coordinar estas operaciones.
/// </summary>
public class EnviarFormularioHandler
{
    private readonly IInvitadoRepository _invitadoRepository;
    private readonly IEmailService _emailService;

    public EnviarFormularioHandler(
        IInvitadoRepository invitadoRepository,
        IEmailService emailService)
    {
        _invitadoRepository = invitadoRepository;
        _emailService = emailService;
    }

    public async Task HandleAsync(
        EnviarFormularioCommand command,
        CancellationToken cancellationToken = default)
    {
        // Guid.NewGuid().ToString("N") genera una cadena de 32 caracteres hexadecimales
        // sin guiones, por ejemplo: "a3f2c1d4e5b6789012345678abcdef01".
        // Es suficientemente único e impredecible para este uso.
        var token = Guid.NewGuid().ToString("N");

        var invitado = new Invitado
        {
            TokenAcceso = token,
            Nombre = command.Nombre,
            Apellidos = command.Apellidos,
            Email = command.Email,
            Asistencia = command.Asistencia,
            CondicionesAlimentarias = command.CondicionesAlimentarias,
            AutobusIda = command.AutobusIda,
            AutobusVuelta = command.AutobusVuelta,
            Cancion = command.Cancion,
            FechaEnvio = DateTime.UtcNow
        };

        // Persistimos primero en BD. Si el INSERT falla, no se envía ningún email:
        // es preferible que el invitado no reciba confirmación a que reciba un enlace
        // de modificación que apunta a un registro que no existe en la BD.
        await _invitadoRepository.AddAsync(invitado, cancellationToken);
        await _invitadoRepository.SaveChangesAsync(cancellationToken);

        // Lanzamos ambos emails en paralelo con Task.WhenAll: el email al invitado
        // y la notificación a los organizadores son independientes entre sí, por lo
        // que no tiene sentido esperar a que uno termine antes de lanzar el otro.
        await Task.WhenAll(
            _emailService.EnviarConfirmacionInvitadoAsync(invitado, cancellationToken),
            _emailService.EnviarNotificacionOrganizadoresAsync(
                $"{invitado.Nombre} {invitado.Apellidos}",
                esModificacion: false,
                cancellationToken)
        );
    }
}