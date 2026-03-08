using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using WebBoda.Application.Interfaces;
using WebBoda.Domain.Interfaces;
using WebBoda.Infrastructure.Services;
using WebBoda.Infrastructure.Configuration;
using WebBoda.Infrastructure.Persistence;
using WebBoda.Infrastructure.Persistence.Repositories;

namespace WebBoda.Infrastructure;

/// <summary>
/// Clase de extensión que centraliza el registro de todos los servicios
/// de la capa Infrastructure en el contenedor de inyección de dependencias de .NET.
///
/// Al encapsular este registro aquí, el Program.cs de la API solo necesita
/// llamar a "builder.Services.AddInfrastructure(configuration)" y no tiene
/// que saber nada sobre EF Core, SMTP ni ningún otro detalle técnico.
/// Esto mantiene la capa API limpia y desacoplada de los detalles de implementación.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // --- Base de datos ---
        // Registramos el DbContext con el proveedor de PostgreSQL (Npgsql).
        // La cadena de conexión se lee del appsettings.json o de variables de entorno.
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(
                    typeof(AppDbContext).Assembly.FullName)));

        // --- Repositorios ---
        // Registramos la implementación concreta asociada a la interfaz de dominio.
        // AddScoped significa que se crea una instancia por petición HTTP,
        // lo cual es el ciclo de vida correcto para repositorios con EF Core.
        services.AddScoped<IInvitadoRepository, InvitadoRepository>();

        // --- Servicio de email ---
        // Leemos la configuración SMTP del appsettings y la registramos
        // con el patrón IOptions<T> para que se pueda inyectar de forma tipada.
        // System.Net.Mail está incluido en .NET de forma nativa, sin paquetes extra.
        services.Configure<SmtpSettings>(options =>
            configuration.GetSection("Smtp").Bind(options));
        services.AddScoped<IEmailService, SmtpEmailService>();

        return services;
    }
}
