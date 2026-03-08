using WebBoda.Domain.Interfaces;

namespace WebBoda.API.Middleware;

/// <summary>
/// Middleware que intercepta las peticiones a los endpoints que requieren token
/// (GET y PUT /api/invitados/{token}) y verifica que el token existe en BD.
///
/// El endpoint de creación (POST /api/invitados) queda completamente libre:
/// cualquier persona puede enviar el formulario sin autenticación previa.
///
/// La estrategia de detección es sencilla y deliberada: si la ruta contiene
/// "/api/invitados/" seguido de algo (el token), se valida. Si la ruta es
/// exactamente "/api/invitados" (el POST de creación), se deja pasar.
/// Este enfoque es robusto y fácil de razonar sin necesidad de atributos
/// personalizados ni filtros de acción adicionales.
/// </summary>
public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;

    public TokenValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IInvitadoRepository invitadoRepository)
    {
        var ruta = context.Request.Path.Value ?? string.Empty;
        var metodo = context.Request.Method;

        // Determinamos si esta petición requiere validación de token.
        // Solo las peticiones a /api/invitados/{algo} (con segmento adicional)
        // son GET o PUT sobre un recurso existente identificado por token.
        var requiereToken = ruta.StartsWith("/api/invitados/", StringComparison.OrdinalIgnoreCase)
                            && (metodo == HttpMethods.Get || metodo == HttpMethods.Put);

        if (requiereToken)
        {
            // Extraemos el token directamente de la URL (es el último segmento).
            var token = ruta.Split('/').LastOrDefault();

            if (string.IsNullOrWhiteSpace(token))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Token no proporcionado." });
                return;
            }

            var invitado = await invitadoRepository.GetByTokenAsync(token);

            if (invitado is null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Token no válido." });
                return;
            }

            // Guardamos el token validado en el contexto para que el controlador
            // pueda usarlo sin necesidad de volver a leer la URL.
            context.Items["TokenAcceso"] = token;
        }

        await _next(context);
    }
}