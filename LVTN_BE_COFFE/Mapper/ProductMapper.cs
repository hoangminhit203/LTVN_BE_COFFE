using AutoMapper;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures.Entities;

namespace LVTN_BE_COFFE.Mapper
{
    public class ProductMapper : Profile
    {
        public ProductMapper()
        {
            // Request -> Entity
            CreateMap<ProductRequest, Products>();

            // Entity -> Response
            CreateMap<Products, ProductResponse>();
        }
    }
}
