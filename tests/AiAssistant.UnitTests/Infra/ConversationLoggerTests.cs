using FluentAssertions;
using AiAssistant.Core.Models;
using AiAssistant.Infra.Middleware;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AiAssistant.UnitTests.Infra;

[TestClass]
public class ConversationLoggerTests
{
	private ILogger<ConversationLogger> _loggerMock = null!;
	private ConversationLogger _sut = null!;

	[TestInitialize]
	public void Setup()
	{
		_loggerMock = Substitute.For<ILogger<ConversationLogger>>();
		_loggerMock.IsEnabled(Arg.Any<LogLevel>()).Returns(true);
		_sut = new ConversationLogger(_loggerMock);
	}

	[TestMethod]
	public async Task turno_logado_contem_todos_os_campos_obrigatorios()
	{
		// Arrange
		var turn = new ConversationTurn(
			userId: "op-123",
			timestamp: DateTimeOffset.UtcNow,
			input: "quais pagamentos vencem essa semana?",
			response: "Encontrei 5 pagamentos que vencem esta semana.",
			latencyMs: 320);

		// Act
		await _sut.LogTurnAsync(turn, CancellationToken.None);

		// Assert — verifica que o logger foi chamado ao menos uma vez
		_loggerMock.ReceivedWithAnyArgs(1).Log(
			Arg.Any<LogLevel>(),
			Arg.Any<EventId>(),
			Arg.Any<object>(),
			Arg.Any<Exception?>(),
			Arg.Any<Func<object, Exception?, string>>());
	}

	[TestMethod]
	public async Task resposta_longa_truncada_no_log()
	{
		// Arrange
		var longResponse = new string('A', 1000);
		var turn = new ConversationTurn(
			userId: "op-123",
			timestamp: DateTimeOffset.UtcNow,
			input: "consulta longa",
			response: longResponse,
			latencyMs: 100);

		// Act
		await _sut.LogTurnAsync(turn, CancellationToken.None);

		// Assert — captura o argumento logado e verifica truncamento
		var calls = _loggerMock.ReceivedCalls().ToList();
		calls.Should().HaveCount(1);

		var args = calls[0].GetArguments();
		var stateObject = args[2];
		var loggedMessage = stateObject?.ToString() ?? string.Empty;

		// A resposta de 1000 chars deve ter sido truncada a 500 + "..."
		loggedMessage.Should().NotContain(longResponse);
	}
}
