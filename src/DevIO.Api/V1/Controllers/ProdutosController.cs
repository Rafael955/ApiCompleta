using AutoMapper;
using DevIO.Api.Controllers;
using DevIO.Api.DTOs;
using DevIO.Business.Intefaces;
using DevIO.Business.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DevIO.Api.V1.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ProdutosController : MainController
    {
        private readonly IProdutoRepository _produtoRepository;
        private readonly IProdutoService _produtoService;
        private readonly IMapper _mapper;

        public ProdutosController(INotificador notificador,
                                  IProdutoRepository produtoRepository,
                                  IProdutoService produtoService,
                                  IMapper mapper,
                                  IUser appUser) : base(notificador, appUser)
        {
            _produtoRepository = produtoRepository;
            _produtoService = produtoService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<ProdutoViewModel>> ObterTodos(CancellationToken cancellationToken)
        {
            try
            {
                return await Task.Run(async () =>
                {
                    //await Task.Delay(10000);
                    return _mapper.Map<IEnumerable<ProdutoViewModel>>(await _produtoRepository.ObterProdutosFornecedores());
                }, cancellationToken);
            }
            catch (TaskCanceledException)
            {
            }

            return Enumerable.Empty<ProdutoViewModel>();
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> ObterPorId(Guid id)
        {
            var produtoDto = await ObterProduto(id);

            if (produtoDto == null)
                return NotFound();

            return produtoDto;
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoViewModel>> Adicionar(ProdutoViewModel produtoDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var imagemNome = Guid.NewGuid() + "_" + produtoDto.Imagem;

            if (!UploadArquivo(produtoDto.ImagemUpload, imagemNome))
            {
                return CustomResponse(produtoDto);
            }

            produtoDto.Imagem = imagemNome;

            await _produtoService.Adicionar(_mapper.Map<Produto>(produtoDto));

            return CustomResponse(produtoDto);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Atualizar(Guid id, ProdutoViewModel produtoDto)
        {
            if (id != produtoDto.Id)
            {
                NotificarErro("Os ids informados não são iguais!");
                return CustomResponse();
            }

            var produtoAtualizacao = await ObterProduto(id);

            if (string.IsNullOrEmpty(produtoDto.Imagem))
                produtoDto.Imagem = produtoAtualizacao.Imagem;

            if (!ModelState.IsValid) return CustomResponse(ModelState);

            if (produtoDto.ImagemUpload != null)
            {
                var imagemNome = Guid.NewGuid() + "_" + produtoDto.Imagem;
                if (!UploadArquivo(produtoDto.ImagemUpload, imagemNome))
                {
                    return CustomResponse(ModelState);
                }

                produtoAtualizacao.Imagem = imagemNome;
            }

            produtoAtualizacao.FornecedorId = produtoDto.FornecedorId;
            produtoAtualizacao.Nome = produtoDto.Nome;
            produtoAtualizacao.Descricao = produtoDto.Descricao;
            produtoAtualizacao.Valor = produtoDto.Valor;
            produtoAtualizacao.Ativo = produtoDto.Ativo;

            await _produtoService.Atualizar(_mapper.Map<Produto>(produtoAtualizacao));

            return CustomResponse(produtoDto);
        }

        [HttpPost]
        public async Task<ActionResult<ProdutoViewModel>> AdicionarAlternativo(ProdutoViewModel produtoDto, CancellationToken token)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            try
            {
                var imagemPrefixo = Guid.NewGuid() + "_";

                if (!await UploadArquivoAlternativo(produtoDto.ImagemPesadaUpload, imagemPrefixo))
                {
                    return CustomResponse(produtoDto);
                }

                produtoDto.Imagem = imagemPrefixo + produtoDto.ImagemPesadaUpload.FileName;

                await Task.Run(() =>
                {
                    _produtoService.Adicionar(_mapper.Map<Produto>(produtoDto));
                }, token);
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine(ex);
                throw;
            }

            return CustomResponse(produtoDto);
        }

        //[DisableRequestSizeLimit]
        //[RequestSizeLimit(40000000)]
        //[HttpPost("imagem")]
        //public async Task<ActionResult> AdicionarImagem(IFormFile imagem)
        //{
        //    return Ok(imagem);
        //}

        [HttpPut]
        public async Task<ActionResult<ProdutoViewModel>> Atualizar(ProdutoViewModel produtoDto)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            await _produtoService.Atualizar(_mapper.Map<Produto>(produtoDto));

            return CustomResponse(produtoDto);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ProdutoViewModel>> Excluir(Guid id)
        {
            var produtoDto = await ObterProduto(id);

            if (produtoDto == null) return NotFound();

            await _produtoService.Remover(id);

            return CustomResponse(produtoDto);
        }

        private async Task<ProdutoViewModel> ObterProduto(Guid id)
        {
            return _mapper.Map<ProdutoViewModel>(await _produtoRepository.ObterProdutoFornecedor(id));
        }

        private bool UploadArquivo(string arquivo, string imgNome)
        {
            if (string.IsNullOrEmpty(arquivo))
            {
                //ModelState.AddModelError(string.Empty, "Forneça uma imagem para este produto");
                NotificarErro("Forneça uma imagem para este produto");
                return false;
            }

            var imageDataByteArray = Convert.FromBase64String(arquivo);

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/app/demo-webapi/src/assets", imgNome);

            if (System.IO.File.Exists(filePath))
            {
                //ModelState.AddModelError(string.Empty, "Já existe um arquivo com este nome!");
                NotificarErro("Já existe um arquivo com este nome!");
                return false;
            }

            System.IO.File.WriteAllBytes(filePath, imageDataByteArray);

            return true;
        }

        private async Task<bool> UploadArquivoAlternativo(IFormFile arquivo, string imgPrefixo)
        {
            if (arquivo == null || arquivo.Length <= 0)
            {
                ModelState.AddModelError(string.Empty, "Forneça uma imagem para este produto");
                return false;
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/app/demo-webapi/src/assets", imgPrefixo + arquivo.FileName);

            if (System.IO.File.Exists(filePath))
            {
                ModelState.AddModelError(string.Empty, "Já existe um arquivo com este nome!");
                return false;
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            return true;
        }
    }
}