namespace WebBoda.Infrastructure.Configuration;

/// <summary>
/// Clase POCO que representa la sección "Smtp" del appsettings.json.
/// </summary>
public class SmtpSettings
{
    /// <summary>Servidor SMTP. Para Outlook es "smtp-mail.outlook.com".</summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>Puerto del servidor SMTP. Outlook usa el 587 con STARTTLS.</summary>
    public int Port { get; set; } = 587;

    /// <summary>Indica si se debe usar cifrado SSL/TLS en la conexión.</summary>
    public bool UseSsl { get; set; } = true;

    /// <summary>
    /// Dirección de email de la cuenta de Outlook remitente.
    /// También actúa como nombre de usuario para autenticarse en el servidor SMTP.
    /// </summary>
    public string Remitente { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña de la cuenta de Outlook.
    /// NUNCA debe estar en el código ni en el appsettings.json subido al repositorio.
    /// Usar User Secrets en desarrollo y variables de entorno en producción.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    // Credenciales OAuth 2.0 registradas en Azure.
    // ClientId identifica la aplicación (quién eres).
    // TenantId identifica el directorio de Microsoft (en qué "mundo" vives).
    // ClientSecret es la contraseña de la aplicación (no del usuario).
    public string ClientId { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>Email de los organizadores que recibirán las notificaciones.</summary>
    public string DestinatarioNotificaciones { get; set; } = string.Empty;

    /// <summary>
    /// URL base del frontend. Se usa para construir el enlace de modificación
    /// que se incluye en el email al invitado.
    /// Ejemplo: "https://www.tuboda.com" en producción, "http://localhost:3000" en desarrollo.
    /// </summary>
    public string FrontendUrl { get; set; } = string.Empty;
}