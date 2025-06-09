using Arquetipo.Api.Configuration;
using Arquetipo.Api.Handlers;
using Arquetipo.Api.HttpHandlers;
using Arquetipo.Api.Infrastructure;
using Arquetipo.Api.Infrastructure.Persistence;
using Arquetipo.Api.Middlewares;
using Arquetipo.Api.Models.Response;
using Arquetipo.Api.Services;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddDbContext<ArquetipoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Database"))
);

builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IClienteHandler, ClienteHandler>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddTransient<ForwardRequestIdDelegatingHandler>();
builder.Services.AddTransient<ITokenService, TokenService>();

string? operacionesApiHost = builder.Configuration["ApiOperaciones:Url"];
if (string.IsNullOrEmpty(operacionesApiHost))
{
    throw new InvalidOperationException("La configuración 'ApiOperaciones:Url' no puede ser nula o vacía.");
}

builder.Services.AddHttpClient<IApiOperacionesClient, OperacionesApiClient>(client =>
{
    client.BaseAddress = new Uri(operacionesApiHost);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
})
.AddHttpMessageHandler<ForwardRequestIdDelegatingHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirApiRequest", policy =>
        policy.WithOrigins("*")
              .WithMethods("GET", "POST", "PUT", "DELETE")
              .AllowAnyHeader());
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddMvc()
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Issuer"],
        ValidateLifetime = true
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = async context =>
        {
            var userRepository = context.HttpContext.RequestServices.GetRequiredService<IUsuarioRepository>();
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<JwtBearerEvents>>();

            var userNameClaim = context.Principal.FindFirst(JwtRegisteredClaimNames.Sub);

            if (userNameClaim == null || string.IsNullOrEmpty(userNameClaim.Value))
            {
                logger.LogWarning("Token validado pero no contiene el claim 'sub' (NombreUsuario).");
                context.Fail("Token inválido: Falta identificador de usuario.");
                return;
            }

            var nombreUsuario = userNameClaim.Value;
            Usuario usuario = await userRepository.GetUsuarioPorNombreAsync(nombreUsuario);

            if (usuario == null || !usuario.EstaActivo)
            {
                logger.LogWarning("Autenticación fallida para el usuario (desde token): {UserIdentifier}. Usuario no encontrado o inactivo.", nombreUsuario);
                context.Fail("Usuario no autorizado o inactivo.");
            }
            else
            {
                logger.LogInformation("Token validado exitosamente contra la base de datos para el usuario: {UserIdentifier}", nombreUsuario);
            }
        }
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseRequestId(); // Middleware para generar y propagar X-Request-ID
app.UseMiddleware<GlobalExceptionHandlerMiddleware>(); // Middleware para manejo de excepciones globales

app.UseSwagger();
var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
List<string> versionStrings = [.. apiVersionDescriptionProvider.ApiVersionDescriptions
    .Select(description => description.ApiVersion.ToString())
    .Distinct()
    .OrderBy(version => version)];

app.MapScalarApiReference(options =>
{
    options
        .AddDocument("v1", "v1", "swagger/v1/swagger.json")
        .AddDocument("v2", "v2", "swagger/v2/swagger.json")
        .Theme = ScalarTheme.Default;
});

app.UseRouting();
app.UseHttpsRedirection();

app.UseCors("PermitirApiRequest");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }