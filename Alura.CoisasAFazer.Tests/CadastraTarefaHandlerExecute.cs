using Alura.CoisasAFazer.Core.Commands;
using Alura.CoisasAFazer.Core.Models;
using Alura.CoisasAFazer.Infrastructure;
using Alura.CoisasAFazer.Services.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Alura.CoisasAFazer.Tests
{
    public class CadastraTarefaHandlerExecute
    {
        [Fact]
        public void DataTareaComInformacoesValidasDeveIncluirNoBancoDeDados()
        {
            // arrange
            var comando = new CadastraTarefa("Estudar xUnit", new Categoria("Estudo"), new DateTime(2019, 12, 1));

            var mock = new Mock<ILogger<CadastraTarefaHandler>>();

            var options = new DbContextOptionsBuilder<DbTarefasContext>()
                .UseInMemoryDatabase("DbTarefasContext")
                .Options;
            var contexto = new DbTarefasContext();
            var repo = new RepositorioTarefa(contexto);

            var handler = new CadastraTarefaHandler(repo, mock.Object);

            // Act
            handler.Execute(comando);

            // Asert
            var tarefa = repo.ObtemTarefas(t => t.Titulo == "Estudar xUnit").FirstOrDefault();
            Assert.NotNull(tarefa);
        }

        [Fact]
        public void QuandoExceptionForLancadaResultadoDeveSerFalso()
        {
            // arrange
            var comando = new CadastraTarefa("Estudar xUnit", new Categoria("Estudo"), new DateTime(2019, 12, 1));

            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            var mock = new Mock<IRepositorioTarefas>();

            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>()))
                .Throws(new Exception("Houe um erro na inclusão de tarefas"));

            var repo = mock.Object;

            var handler = new CadastraTarefaHandler(repo, mockLogger.Object);

            // Act
            CommandResult resultado = handler.Execute(comando);

            // Asert
            Assert.False(resultado.IsSuccess);
        }

        [Fact]
        public void QuandoExceptionForLancadaDeveLogarAMensagemDaException()
        {
            // arrange
            var excecaoEsperada = new Exception("Houe um erro na inclusão de tarefas");
            var comando = new CadastraTarefa("Estudar xUnit", new Categoria("Estudo"), new DateTime(2019, 12, 1));

            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();

            var mock = new Mock<IRepositorioTarefas>();

            mock.Setup(r => r.IncluirTarefas(It.IsAny<Tarefa[]>()))
                .Throws(excecaoEsperada);

            var repo = mock.Object;

            var handler = new CadastraTarefaHandler(repo, mockLogger.Object);

            // Act
            CommandResult resultado = handler.Execute(comando);

            // Asert
            mockLogger.Verify(l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    excecaoEsperada,
                    It.IsAny<Func<object, Exception, string>>()
                ), Times.Once());
        }

        delegate void CapturaMensagemlog(LogLevel level, EventId eventId, object state, Exception exception,
            Func<object, Exception, string> function);

        [Fact]
        public void DataTarefaComInfoValidaDeveLogar()
        {
            // arrange
            var comando = new CadastraTarefa("Estudar xUnit", new Categoria("Estudo"), new DateTime(2019, 12, 1));

            LogLevel levelCapturado = LogLevel.Error;
            string mensagemCapturada = string.Empty;

            CapturaMensagemlog captura = (level, eventId, state, exception, function) =>
            {
                levelCapturado = level;
                mensagemCapturada = function(state, exception);
            };

            var mockLogger = new Mock<ILogger<CadastraTarefaHandler>>();
            mockLogger.Setup(l => l.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<object, Exception, string>>()
                )).Callback(captura);
            
            var mockRepo = new Mock<IRepositorioTarefas>();

            var handler = new CadastraTarefaHandler(mockRepo.Object, mockLogger.Object);

            // Act
            handler.Execute(comando);

            // Asert
            Assert.Equal(LogLevel.Debug, levelCapturado);
            Assert.Contains(comando.Titulo, mensagemCapturada);
        }
    }
}
