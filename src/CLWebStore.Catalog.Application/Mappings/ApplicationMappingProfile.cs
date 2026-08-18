using AutoMapper;
using CLWebStore.Catalog.Application.DTOs.V1;
using CLWebStore.Catalog.Domain.Aggregates;
using CLWebStore.Catalog.Domain.Entities;

namespace CLWebStore.Catalog.Application.Mappings;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        // Domain Entity -> Application DTO (Read-only view)
        CreateMap<Product, ProductDto>()
            .ForMember(d => d.Sku, opt => opt.MapFrom(s => s.Sku.Value))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name.Value))
            .ForMember(d => d.PriceAmount, opt => opt.MapFrom(s => s.Price.Amount))
            .ForMember(d => d.PriceCurrency, opt => opt.MapFrom(s => s.Price.Currency));

        CreateMap<ProductImage, ProductImageDto>();
    }
}