using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Alura.CoisasAFazer.WebApp.Controllers;
using Alura.CoisasAFazer.WebApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Alura.CoisasAFazer.Tests
{
    public class TarefasControllerEndpointCadastraTarefa
    {
        [Fact]
        public void DadaTarefaComInformacoesValidasDeveRetornarOkResult()
        {
            // arrange
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            var options = new DbContextOptionsBuilder<DbTarefasContext>()
                .UseInMemoryDatabase("DbTarefasContext")
                .Options;
            var contexto = new DbTarefasContext(options);
            contexto.Categorias.Add(new Core.Models.Categoria(20, "Estudo"));
            contexto.SaveChanges();

            var repo = new RepositorioTarefa(contexto);

            var controlador = new TarefasController(repo, mockLogger.Object);
            var model = new CadastraTarefaVM();
            model.IdCategoria = 20;
            model.Titulo = "Estudar xUnit";
            model.Prazo = new DateTime(2019, 12, 31);

            // act
            var result = controlador.EndpointCadastraTarefa(model);

            // assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void QuandoExcecaoForLancadaDeveRetornarBadRequestResult()
        {
            // arrange
            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            var mockRepo = new Mock<IRepositorioTarefas>();
            mockRepo.Setup(r => r.ObtemCategoriaPorId(20)).Returns(new Categoria(20, "Estudo"));
            mockRepo.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>()))
                .Throws(new Exception("Houve um erro ao incluir as tarefas"));

            var controlador = new TarefasController(mockRepo.Object, mockLogger.Object);
            var model = new CadastraTarefaVM();
            model.IdCategoria = 20;
            model.Titulo = "Estudar xUnit";
            model.Prazo = new DateTime(2019, 12, 31);

            // act
            var result = controlador.EndpointCadastraTarefa(model);

            // assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
