using AutoMapper;
using CLWebStore.Catalog.API.Contracts.V1.Requests;
using CLWebStore.Catalog.Application.Commands.V1.CreateProduct;
using CLWebStore.Catalog.Application.Commands.V1.UpdateProduct;
using CLWebStore.Catalog.Application.DTOs.V1;

namespace CLWebStore.Catalog.API.Mappings;

public class ApiMappingProfile : Profile
{
    public ApiMappingProfile()
    {
        // Map purely by matching property names
        CreateMap<ProductImageRequest, CreateProductImageDto>();
        CreateMap<CreateProductRequest, CreateProductCommand>();

        CreateMap<UpdateProductImageRequest, UpdateProductImageDto>();

        // The ID is passed via the route, not the request body, so it must be explicitly ignored
        CreateMap<UpdateProductRequest, UpdateProductCommand>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
