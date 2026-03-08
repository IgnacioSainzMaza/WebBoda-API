using WebBoda.Domain.Enums;

namespace WebBoda.Domain.Entities;

/// <summary>
/// Representa la respuesta de un invitado al formulario de la boda.
///
/// A diferencia del diseño anterior (donde el invitado existía en BD antes
/// de rellenar el formulario), ahora la entidad se crea en el momento en que
/// el invitado envía el formulario por primera vez. No hay registros precargados.
///
/// El TokenAcceso sigue existiendo, pero nace en el momento del primer envío:
/// el backend lo genera, lo guarda en BD y se lo envía al invitado por email
/// para que pueda modificar sus respuestas en el futuro si lo necesita.
/// </summary>
public class Invitado
{
    public int Id { get; set; }

    /// <summary>
    /// Token único generado en el momento del primer envío del formulario.
    /// Se incluye en el email de confirmación como parte del enlace de modificación:
    /// "miboda.com/modificar/{TokenAcceso}"
    /// </summary>
    public string TokenAcceso { get; set; } = string.Empty;

    // --- Datos de identidad ---
    public string Nombre { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;

    /// <summary>
    /// Email del invitado. Se usa para enviarle el resumen de sus respuestas
    /// y el enlace de modificación. Es el único dato de contacto que tenemos.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    // --- Campos del formulario ---

    /// <summary>
    /// Indica si el invitado ha confirmado su asistencia a la boda.
    /// </summary>
    public bool Asistencia { get; set; }

    /// <summary>
    /// Alergias, intolerancias u otras condiciones alimentarias del invitado.
    /// Campo libre de texto. Null si no tiene ninguna.
    /// </summary>
    public string? CondicionesAlimentarias { get; set; }

    /// <summary>
    /// Indica si el invitado quiere plaza en el autobús de ida al evento.
    /// </summary>
    public bool AutobusIda { get; set; }

    /// <summary>
    /// Opción de autobús de vuelta elegida por el invitado.
    /// Se almacena como int en PostgreSQL (0, 1 o 2).
    /// </summary>
    public TipoAutobusVuelta AutobusVuelta { get; set; } = TipoAutobusVuelta.NoCogeAutobus;

    /// <summary>
    /// Canción que el invitado propone para la boda.
    /// </summary>
    public string? Cancion { get; set; }

    // --- Metadatos de auditoría ---

    /// <summary>
    /// Fecha en la que el invitado envió el formulario por primera vez.
    /// </summary>
    public DateTime FechaEnvio { get; set; }

    /// <summary>
    /// Fecha de la última modificación del formulario.
    /// Null si el invitado nunca ha modificado su respuesta inicial.
    /// </summary>
    public DateTime? FechaUltimaModificacion { get; set; }
}
