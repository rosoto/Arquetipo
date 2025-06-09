using Arquetipo.Api.Models.Response.ApiOperaciones;

namespace Arquetipo.Api.Services
{
    public interface IApiOperacionesClient
    {
        Task<OperacionesApiResponse<TasaDeCambioItem>> GetTasaDeCambioAsync(DateTime fechaConsulta, string codigoMoneda);
        Task<OperacionesApiResponse<FeriadoLegalItem>> GetFeriadosLegalesAsync(DateTime fechaInicio, DateTime fechaFin);
    }
}
