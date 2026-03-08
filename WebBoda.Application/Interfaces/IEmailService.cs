using WebBoda.Domain.Entities;

namespace WebBoda.Application.Interfaces;

/// <summary>
/// Contrato para el servicio de envío de emails.
///
/// Definimos dos métodos con responsabilidades claramente separadas:
///   - EnviarConfirmacionInvitadoAsync: dirigido al invitado, incluye su resumen y el enlace.
///   - EnviarNotificacionOrganizadoresAsync: dirigido a los organizadores, les informa del evento.
///
/// Ambos métodos reciben los datos que necesitan y nada más, siguiendo el principio
/// de mínimo acoplamiento: la implementación concreta (SmtpEmailService) es libre
/// de formatear el email como quiera sin que Application sepa nada de ello.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envía al invitado un email con el resumen de todas sus respuestas
    /// y un enlace para modificarlas en el futuro.
    /// Se llama tanto en el primer envío como en cada modificación posterior,
    /// de modo que el invitado siempre tiene en su bandeja de entrada
    /// el estado actualizado de sus respuestas.
    /// </summary>
    Task EnviarConfirmacionInvitadoAsync(
        Invitado invitado,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envía una notificación a los organizadores de la boda cuando un invitado
    /// envía o modifica su formulario.
    /// </summary>
    /// <param name="nombreInvitado">Nombre completo del invitado.</param>
    /// <param name="esModificacion">True si el invitado ya había respondido antes.</param>
    Task EnviarNotificacionOrganizadoresAsync(
        string nombreInvitado,
        bool esModificacion,
        CancellationToken cancellationToken = default);
}