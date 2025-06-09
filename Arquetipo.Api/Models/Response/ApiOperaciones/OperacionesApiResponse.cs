using System.Text.Json.Serialization;

namespace Arquetipo.Api.Models.Response.ApiOperaciones
{
    public class OperacionesApiResponse<T>
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("comentario")]
        public string? Comentario { get; set; }

        [JsonPropertyName("sessionId")]
        public required string SessionId { get; set; }

        [JsonPropertyName("data")]
        public List<T>? Data { get; set; }
    }

    public class TasaDeCambioItem
    {
        [JsonPropertyName("tasacambio")]
        public decimal TasaCambio { get; set; }

        [JsonPropertyName("fechacambio")]
        public string? FechaCambio { get; set; }

        [JsonPropertyName("par_cod_error")]
        public int ParCodError { get; set; }

        [JsonPropertyName("par_msg_error")]
        public string? ParMsgError { get; set; }
    }

    public class FeriadoLegalItem
    {
        [JsonPropertyName("anio")]
        public int Anio { get; set; }

        [JsonPropertyName("mes")]
        public int Mes { get; set; }

        [JsonPropertyName("dia")]
        public int Dia { get; set; }

        [JsonPropertyName("diaSemana")]
        public required string DiaSemana { get; set; }

        [JsonPropertyName("esFeriado")]
        public required string EsFeriado { get; set; }
    }
}
