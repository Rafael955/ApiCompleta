using System.ComponentModel.DataAnnotations;

namespace DevIO.Business.Models
{
    public enum TipoFornecedor
    {
        [Display(Name = "Pessoa Física")]
        PessoaFisica = 1,
        [Display(Name = "Pessoa Jurídica")]
        PessoaJuridica
    }
}