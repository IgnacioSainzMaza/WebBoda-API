using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using WebBoda.Infrastructure.Persistence;

namespace WebBoda.Infrastructure.Persistence;

/// <summary>
/// Fábrica que la herramienta de migraciones de EF Core usa en tiempo de diseño
/// para construir el AppDbContext sin necesidad de arrancar la aplicación completa.
///
/// ¿Por qué es necesaria esta clase?
/// Cuando ejecutas "Add-Migration" o "Update-Database", la herramienta dotnet-ef
/// intenta construir el DbContext arrancando el proyecto en un modo especial.
/// En proyectos con Clean Architecture, la cadena de conexión vive en User Secrets
/// o en variables de entorno, y ese proceso puede fallar si no están disponibles.
/// Esta fábrica le da a la herramienta una vía alternativa y directa para construir
/// el contexto con una cadena de conexión local hardcodeada.
///
/// IMPORTANTE: Esta clase SOLO se usa durante el desarrollo para generar migraciones.
/// En producción, el contexto se construye a través de la inyección de dependencias
/// registrada en DependencyInjection.cs, y esta clase queda completamente ignorada.
/// La cadena de conexión hardcodeada aquí es segura porque nunca llega a producción.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

        // Ajusta el usuario y la contraseña a los de tu instalación local de PostgreSQL.
        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=webboda_db;Username=postgres;Password=admin");

        return new AppDbContext(optionsBuilder.Options);
    }
}