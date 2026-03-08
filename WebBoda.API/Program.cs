using WebBoda.API.Middleware;
using WebBoda.Application.Invitados.Commands.EnviarFormulario;
using WebBoda.Application.Invitados.Commands.ModificarFormulario;
using WebBoda.Application.Invitados.Queries.GetInvitadoByToken;
using WebBoda.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// -------------------------------------------------------------------------
// REGISTRO DE SERVICIOS
// -------------------------------------------------------------------------

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddScoped<GetInvitadoByTokenHandler>();
builder.Services.AddScoped<EnviarFormularioHandler>();
builder.Services.AddScoped<ModificarFormularioHandler>();

builder.Services.AddControllers();

// Configuración de CORS.
// La URL del frontend se lee de la configuración para que sea fácil
// cambiarla entre entornos sin tocar el código:
//   - En desarrollo: appsettings.Development.json → "http://localhost:4200"
//   - En producción: variable de entorno en Azure → la URL real del frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .WithOrigins(
                builder.Configuration["Cors:FrontendUrl"] ?? "http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// -------------------------------------------------------------------------
// PIPELINE DE MIDDLEWARES
// -------------------------------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// CORS debe ir lo antes posible en el pipeline, antes incluso de la
// redirección HTTPS, para que las peticiones preflight (OPTIONS) que
// el navegador envía antes de cada petición cross-origin reciban
// las cabeceras correctas sin ser redirigidas primero.
app.UseCors("FrontendPolicy");

app.UseHttpsRedirection();

app.UseMiddleware<TokenValidationMiddleware>();

app.MapControllers();

app.Run();