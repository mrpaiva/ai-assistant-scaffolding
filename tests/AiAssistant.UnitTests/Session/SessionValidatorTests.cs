using FluentAssertions;
using AiAssistant.Infra.Middleware;

namespace AiAssistant.UnitTests.Session;

[TestClass]
public class SessionValidatorTests
{
	private SessionValidator _sut = null!;

	[TestInitialize]
	public void Setup()
	{
		_sut = new SessionValidator();
	}

	[TestMethod]
	public void mesmo_operador_da_sessao_retorna_verdadeiro()
	{
		// Arrange
		var operatorId = "op-123";
		var sessionId = "session-abc";
		_sut.RegisterSession(sessionId, operatorId);

		// Act
		var result = _sut.Validate(operatorId, sessionId);

		// Assert
		result.Should().BeTrue();
	}

	[TestMethod]
	public void operador_diferente_da_sessao_retorna_falso()
	{
		// Arrange
		var sessionId = "session-abc";
		_sut.RegisterSession(sessionId, "op-123");

		// Act
		var result = _sut.Validate("op-456", sessionId);

		// Assert
		result.Should().BeFalse();
	}

	[TestMethod]
	public void sessao_duplicada_nao_sobrescreve_operador()
	{
		var sessionId = "session-abc";
		_sut.RegisterSession(sessionId, "op-123").Should().BeTrue();
		_sut.RegisterSession(sessionId, "op-456").Should().BeFalse();

		_sut.Validate("op-123", sessionId).Should().BeTrue();
		_sut.Validate("op-456", sessionId).Should().BeFalse();
	}

	[TestMethod]
	public void sessao_inexistente_retorna_falso()
	{
		// Arrange & Act
		var result = _sut.Validate("op-123", "sessao-que-nao-existe");

		// Assert
		result.Should().BeFalse();
	}
}
