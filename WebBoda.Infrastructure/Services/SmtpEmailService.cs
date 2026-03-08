using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using WebBoda.Application.Interfaces;
using WebBoda.Domain.Entities;
using WebBoda.Domain.Enums;
using WebBoda.Infrastructure.Configuration;

namespace WebBoda.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    public SmtpEmailService(IOptions<SmtpSettings> settings)
    {
        _settings = settings.Value;
    }

    // -------------------------------------------------------------------------
    // MÉTODO INTERNO DE ENVÍO
    // -------------------------------------------------------------------------
    // Centraliza toda la lógica de conexión SMTP. Recibe el destinatario,
    // asunto, cuerpo y un booleano que indica si el cuerpo es HTML o texto plano.
    // Los dos métodos públicos lo llaman una vez que han preparado el contenido.
    // -------------------------------------------------------------------------
    private async Task EnviarAsync(
        string destinatario,
        string asunto,
        string cuerpo,
        bool esHtml,
        CancellationToken cancellationToken)
    {
        try
        {
            using var cliente = new SmtpClient(_settings.Host, _settings.Port)
            {
                EnableSsl = _settings.UseSsl,
                Credentials = new NetworkCredential(
                    _settings.Remitente,
                    _settings.Password
                )
            };

            var mensaje = new MailMessage
            {
                From = new MailAddress(_settings.Remitente, "Almudena & Ignacio"),
                Subject = asunto,
                Body = cuerpo,
                // Aquí indicamos al cliente de correo si debe renderizar
                // el contenido como HTML o mostrarlo como texto plano.
                // Para la confirmación al invitado usaremos HTML (true).
                // Para la notificación a organizadores, texto plano (false).
                IsBodyHtml = esHtml,
                BodyEncoding = Encoding.UTF8
            };

            mensaje.To.Add(destinatario);
            await cliente.SendMailAsync(mensaje, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar email a {destinatario}: {ex.Message}");
            throw;
        }
    }

    // -------------------------------------------------------------------------
    // CARGA Y PROCESAMIENTO DE LA PLANTILLA HTML
    // -------------------------------------------------------------------------
    // Este método hace tres cosas en orden:
    //   1. Lee el archivo HTML de la plantilla desde disco
    //   2. Sustituye todos los marcadores {{CAMPO}} por valores reales
    //   3. Gestiona los bloques condicionales {{#IF_ASISTE}}...{{/IF_ASISTE}}
    //
    // La ruta de la plantilla se construye de forma relativa al directorio
    // donde está el ejecutable, lo que funciona tanto en desarrollo como
    // en producción (Azure App Service) sin necesidad de rutas absolutas.
    // -------------------------------------------------------------------------
    private async Task<string> ProcesarPlantillaAsync(Invitado invitado)
    {
        // Construimos la ruta al archivo de plantilla de forma robusta.
        // AppContext.BaseDirectory apunta al directorio del ejecutable,
        // que es donde se copiarán los archivos marcados como
        // "Copy to Output Directory" en el proyecto de Visual Studio.
        var rutaPlantilla = Path.Combine(
            AppContext.BaseDirectory,
            "Templates",
            "email_confirmacion.html"
        );

        if (!File.Exists(rutaPlantilla))
            throw new FileNotFoundException(
                $"No se encontró la plantilla de email en: {rutaPlantilla}");

        var html = await File.ReadAllTextAsync(rutaPlantilla);

        // ── PASO 1: Gestionar el bloque condicional ───────────────────────────
        // Si el invitado no asiste, eliminamos todo el contenido entre las
        // etiquetas {{#IF_ASISTE}} y {{/IF_ASISTE}}, incluyendo las propias
        // etiquetas. Si sí asiste, solo eliminamos las etiquetas dejando
        // el contenido interior intacto.
        //
        // RegexOptions.Singleline hace que el punto (.) también coincida
        // con saltos de línea, lo cual es necesario porque el bloque
        // condicional ocupa múltiples líneas en el HTML.
        if (!invitado.Asistencia)
        {
            html = Regex.Replace(
                html,
                @"\{\{#IF_ASISTE\}\}.*?\{\{/IF_ASISTE\}\}",
                string.Empty,
                RegexOptions.Singleline
            );
        }
        else
        {
            // Si asiste, solo quitamos las etiquetas, manteniendo el contenido
            html = html
                .Replace("{{#IF_ASISTE}}", string.Empty)
                .Replace("{{/IF_ASISTE}}", string.Empty);
        }

        // ── PASO 2: Formatear los valores del enum para mostrarlos en el email ─
        var autobusVuelta = invitado.AutobusVuelta switch
        {
            TipoAutobusVuelta.NoCogeAutobus => "No",
            TipoAutobusVuelta.PrimerServicio => "Sí, primer servicio (Zona de Ventas)",
            TipoAutobusVuelta.SegundoServicio => "Sí, segundo servicio (Zona de Pirámides)",
            _ => "No especificado"
        };

        var mensajeAsistencia = invitado.Asistencia
            ? "¡Gracias por confirmar tu asistencia a nuestra boda! Estamos muy felices de que puedas acompañarnos."
            : "Hemos recibido tu respuesta. Lamentamos que no puedas acompañarnos, pero te tendremos muy presente.";

        var enlaceModificacion =
            $"{_settings.FrontendUrl}/modificar/{invitado.TokenAcceso}";

        // ── PASO 3: Sustituir todos los marcadores por valores reales ─────────
        // Cada llamada a Replace busca el marcador exacto en el HTML y lo
        // sustituye por el valor correspondiente. El orden no importa porque
        // los marcadores son únicos y no se solapan entre sí.
        html = html
            .Replace("{{NOMBRE}}", invitado.Nombre)
            .Replace("{{APELLIDOS}}", invitado.Apellidos)
            .Replace("{{EMAIL}}", invitado.Email)
            .Replace("{{ASISTENCIA}}", invitado.Asistencia ? "Sí" : "No")
            .Replace("{{AUTOBUS_IDA}}", invitado.AutobusIda ? "Sí" : "No")
            .Replace("{{AUTOBUS_VUELTA}}", autobusVuelta)
            .Replace("{{CONDICIONES_ALIMENTARIAS}}",
                string.IsNullOrWhiteSpace(invitado.CondicionesAlimentarias)
                    ? "Ninguna"
                    : invitado.CondicionesAlimentarias)
            .Replace("{{MENSAJE_ASISTENCIA}}", mensajeAsistencia)
            .Replace("{{ENLACE_MODIFICACION}}", enlaceModificacion);

        return html;
    }

    // -------------------------------------------------------------------------
    // IMPLEMENTACIÓN DE IEmailService
    // -------------------------------------------------------------------------

    public async Task EnviarConfirmacionInvitadoAsync(
        Invitado invitado,
        CancellationToken cancellationToken = default)
    {
        var asunto = "Confirmación de asistencia — Boda de Almudena e Ignacio";

        // Procesamos la plantilla HTML con los datos reales del invitado
        var cuerpoHtml = await ProcesarPlantillaAsync(invitado);

        // Enviamos como HTML (esHtml: true)
        await EnviarAsync(
            invitado.Email,
            asunto,
            cuerpoHtml,
            esHtml: true,
            cancellationToken);
    }

    public async Task EnviarNotificacionOrganizadoresAsync(
        string nombreInvitado,
        bool esModificacion,
        CancellationToken cancellationToken = default)
    {
        // La notificación a organizadores es texto plano simple.
        // No necesita plantilla HTML porque es un aviso interno,
        // no un documento que vea el invitado.
        var accion = esModificacion ? "modificado" : "enviado";
        var asunto = esModificacion
            ? $"[WebBoda] {nombreInvitado} ha modificado su confirmación"
            : $"[WebBoda] Nueva confirmación de {nombreInvitado}";

        var cuerpo = new StringBuilder();
        cuerpo.AppendLine(
            $"{nombreInvitado} ha {accion} su formulario de confirmación de asistencia.");
        cuerpo.AppendLine();
        cuerpo.AppendLine("Puedes consultar todos los registros en la base de datos.");

        // Enviamos como texto plano (esHtml: false)
        await EnviarAsync(
            _settings.DestinatarioNotificaciones,
            asunto,
            cuerpo.ToString(),
            esHtml: false,
            cancellationToken);
    }
}