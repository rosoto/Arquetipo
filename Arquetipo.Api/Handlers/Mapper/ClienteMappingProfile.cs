using Arquetipo.Api.Models.Request.v1;
using Arquetipo.Api.Models.Request.v2;
using Arquetipo.Api.Models.Request;
using Arquetipo.Api.Models.Response.v1;
using Arquetipo.Api.Models.Response.v2;
using Arquetipo.Api.Models.Response;
using AutoMapper;

namespace Arquetipo.Api.Handlers.Mapper
{
    public class ClienteMappingProfile : Profile
    {
        public ClienteMappingProfile()
        {
            // Mapeos de Request a Modelos de Repositorio/Entidad
            CreateMap<CrearClienteRequestV1, SetCliente>();
            CreateMap<CrearClienteRequestV2, SetCliente>()
                .ForMember(dest => dest.Telefono, opt => opt.MapFrom(src => src.Telefono ?? "N/A")); // Manejo de nulo

            CreateMap<ActualizarClienteRequest, SetCliente>();

            // Mapeos de Entidad a Response DTOs
            CreateMap<Cliente, ClienteResponse>();
            CreateMap<Cliente, ClienteResponseV2>()
                // Aquí puedes configurar valores por defecto para los nuevos campos de V2
                .ForMember(dest => dest.EstadoCivil, opt => opt.MapFrom(src => "No especificado"))
                .ForMember(dest => dest.FechaRegistro, opt => opt.MapFrom(src => DateTime.UtcNow));
        }
    }
}
