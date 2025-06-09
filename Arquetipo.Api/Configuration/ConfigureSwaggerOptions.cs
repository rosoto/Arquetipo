using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Arquetipo.Api.Configuration;

/// <summary>
/// Configura las opciones de SwaggerGen para la aplicación.
/// Esta clase es responsable de generar un documento Swagger por cada versión de API
/// y de configurar la seguridad JWT en la UI de la documentación.
/// </summary>
/// <remarks>
/// Permite que la versionado de API defina un documento Swagger por cada versión de API
/// después de que el servicio IApiVersionDescriptionProvider haya sido resuelto del contenedor de servicios.
/// </remarks>
/// <remarks>
/// Inicializa una nueva instancia de la clase <see cref="ConfigureSwaggerOptions"/>.
/// </remarks>
/// <param name="provider">El <see cref="IApiVersionDescriptionProvider"/> utilizado para generar los documentos Swagger.</param>
public class ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider = provider;

    /// <summary>
    /// Configura la instancia de <see cref="SwaggerGenOptions"/>.
    /// </summary>
    /// <param name="options">Las <see cref="SwaggerGenOptions"/> a configurar.</param>
    public void Configure(SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Encabezado de autorización JWT usando el esquema Bearer. \r\n\r\n " +
                          "Ingresa 'Bearer' [espacio] y luego tu token en la entrada de texto de abajo.\r\n\r\n" +
                          "Ejemplo: \"Bearer 12345abcdef\"",
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });

        foreach (var description in _provider.ApiVersionDescriptions)
            options.SwaggerDoc(description.GroupName, CreateVersionInfo(description));

        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    }

    /// <summary>
    /// Crea la información de <see cref="OpenApiInfo"/> para una versión de API dada.
    /// </summary>
    /// <param name="description">La <see cref="ApiVersionDescription"/> de la API.</param>
    /// <returns>Una instancia de <see cref="OpenApiInfo"/>.</returns>
    private static OpenApiInfo CreateVersionInfo(ApiVersionDescription description) //
    {
        var info = new OpenApiInfo()
        {
            Title = "Arquetipo API",
            Version = description.ApiVersion.ToString(),
            Description = $"Documentación para la API Arquetipo {description.ApiVersion}.",
        };

        if (description.IsDeprecated)
            info.Description += " Esta versión de API ha sido marcada como obsoleta.";

        return info;
    }
}