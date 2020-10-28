using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using DevIO.Business.Models.Validations;

namespace DevIO.Business.Services
{
    public class FornecedorService : BaseService, IFornecedorService
    {
        private readonly IFornecedorRepository _fornecedorRepository;
        private readonly IEnderecoRepository _enderecoRepository;

        public FornecedorService(IFornecedorRepository fornecedorRepository, 
                                 IEnderecoRepository enderecoRepository,
                                 INotificador notificador) : base(notificador)
        {
            _fornecedorRepository = fornecedorRepository;
            _enderecoRepository = enderecoRepository;
        }

        public async Task<bool> Adicionar(Fornecedor fornecedor)
        {
            if (!ExecutarValidacao(new FornecedorValidator(), fornecedor) 
                || !ExecutarValidacao(new EnderecoValidator(), fornecedor.Endereco)) return false;

            if (!ValidarFornecedor(fornecedor, nameof(Adicionar))) return false;

            await _fornecedorRepository.Adicionar(fornecedor);

            return true;
        }

        public async Task<bool> Atualizar(Fornecedor fornecedor)
        {
            if (!ExecutarValidacao(new FornecedorValidator(), fornecedor)) return false;

            if (!ValidarFornecedor(fornecedor, nameof(Atualizar))) return false;

            await _fornecedorRepository.Atualizar(fornecedor);
            return true;
        }

        public async Task AtualizarEndereco(Endereco endereco)
        {
            if (!ExecutarValidacao(new EnderecoValidator(), endereco)) return;

            await _enderecoRepository.Atualizar(endereco);
        }

        public async Task<bool> Remover(Guid id)
        {
            if (_fornecedorRepository.ObterFornecedorProdutosEndereco(id).Result.Produtos.Any())
            {
                Notificar("O fornecedor possui produtos cadastrados!");
                return false;
            }

            var endereco = await _enderecoRepository.ObterEnderecoPorFornecedor(id);

            if (endereco != null)
            {
                await _enderecoRepository.Remover(endereco.Id);
            }

            await _fornecedorRepository.Remover(id);
            return true;
        }

        private bool ValidarFornecedor(Fornecedor fornecedor, string action)
        {
            if(action == nameof(Adicionar))
            {
                if (_fornecedorRepository.Buscar(f => f.Documento == fornecedor.Documento).Result.Any())
                {
                    Notificar("Já existe um fornecedor com este documento informado.");
                    return false;
                }
            }

            if(action == nameof(Atualizar))
            {
                if (_fornecedorRepository.Buscar(f => f.Documento == fornecedor.Documento && f.Id != fornecedor.Id).Result.Any())
                {
                    Notificar("Já existe um fornecedor com este documento infomado.");
                    return false;
                }
            }
            
            return true;
        }

        public void Dispose()
        {
            _fornecedorRepository?.Dispose();
            _enderecoRepository?.Dispose();
        }
    }
}