using AutoMapper;
using DevIO.Api.DTOs;
using DevIO.Business.Models;

namespace DevIO.Api.Configuration
{
    public class AutoMapperConfig : Profile
    {
        // Tudo o que estiver aqui dentro será resolvido pelo AutoMapper assim que a API for inicializada
        public AutoMapperConfig()
        {
            CreateMap<Fornecedor, FornecedorViewModel>().ReverseMap();
            CreateMap<Endereco, EnderecoViewModel>().ReverseMap();

            CreateMap<ProdutoViewModel, Produto>();
            CreateMap<Produto, ProdutoViewModel>()
                .ForMember(dest => dest.NomeFornecedor, opt => opt.MapFrom(src => src.Fornecedor.Nome));
        }
    }
}