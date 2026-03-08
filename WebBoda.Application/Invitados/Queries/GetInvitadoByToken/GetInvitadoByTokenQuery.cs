namespace WebBoda.Application.Invitados.Queries.GetInvitadoByToken;

/// <summary>
/// Representa la petición de consulta para obtener los datos de un invitado
/// a partir de su token de acceso.
///
/// En el patrón CQRS, una Query es una operación de solo lectura: no modifica
/// ningún estado, simplemente recupera y devuelve datos. El frontend la usa
/// para precargar el formulario de modificación con las respuestas anteriores.
/// </summary>
public class GetInvitadoByTokenQuery
{
    /// <summary>
    /// El token único del invitado extraído de la URL.
    /// Ejemplo: en "tuboda.com/modificar/abc123xyz", el token sería "abc123xyz".
    /// </summary>
    public string Token { get; set; } = string.Empty;
}

