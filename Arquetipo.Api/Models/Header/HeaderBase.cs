using System.Net;

namespace Arquetipo.Api.Models.Header;

public class HeaderBase
{
    /// <summary>
    /// Version del Servicio
    /// </summary>
    /// <example>2.0</example>
    public string Version { get; }
    /// <summary>
    /// Status peticion del servicio
    /// </summary>
    /// <example>200</example>
    public int Status { get; set; }
    /// <summary>
    /// Nombre del Host
    /// </summary>
    /// <example>Server01</example>
    public string HostName { get; }
    /// <summary>
    /// Nombre del Path de la ruta de la aplicacion
    /// </summary>
    /// <example>C://Carpeta/Carpeta2/CarpetaAplicaciones</example>
    public string CodeVersion { get; }

    public HeaderBase()
    {
        Version = "0.1";
        Status = 200;
        HostName = Dns.GetHostName();
        CodeVersion = Environment.Version.Revision.ToString();
    }
}