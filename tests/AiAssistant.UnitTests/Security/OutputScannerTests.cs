using FluentAssertions;
using AiAssistant.Infra.Middleware.Security;

namespace AiAssistant.UnitTests.Security;

[TestClass]
public class OutputScannerTests
{
	private OutputScanner _sut = null!;

	[TestInitialize]
	public void Setup()
	{
		_sut = new OutputScanner();
	}

	[TestMethod]
	public void resposta_normal_nao_detecta_vazamento()
	{
		// Arrange
		var response = "Os pagamentos que vencem esta semana totalizam R$ 15.000,00.";

		// Act
		var (leaked, sanitized) = _sut.Scan(response);

		// Assert
		leaked.Should().BeFalse();
		sanitized.Should().BeNull();
	}

	[TestMethod]
	public void referencia_a_instrucoes_internas_detecta_vazamento()
	{
		// Arrange
		var response = "Fui instruído a responder sempre de forma educada e não revelar dados sensíveis.";

		// Act
		var (leaked, sanitized) = _sut.Scan(response);

		// Assert
		leaked.Should().BeTrue();
		sanitized.Should().Be("Desculpe, não posso compartilhar essa informação.");
	}

	[TestMethod]
	public void referencia_ao_system_prompt_detecta_vazamento()
	{
		// Arrange
		var response = "O meu system prompt diz que devo agir como um assistente.";

		// Act
		var (leaked, sanitized) = _sut.Scan(response);

		// Assert
		leaked.Should().BeTrue();
		sanitized.Should().Be("Desculpe, não posso compartilhar essa informação.");
	}

	[TestMethod]
	public void referencia_minhas_instrucoes_detecta_vazamento()
	{
		// Arrange
		var response = "De acordo com minhas instruções, não posso fazer isso.";

		// Act
		var (leaked, sanitized) = _sut.Scan(response);

		// Assert
		leaked.Should().BeTrue();
		sanitized.Should().Be("Desculpe, não posso compartilhar essa informação.");
	}

	[TestMethod]
	public void referencia_meu_prompt_detecta_vazamento()
	{
		// Arrange
		var response = "O meu prompt original contém regras específicas.";

		// Act
		var (leaked, sanitized) = _sut.Scan(response);

		// Assert
		leaked.Should().BeTrue();
		sanitized.Should().Be("Desculpe, não posso compartilhar essa informação.");
	}

	[TestMethod]
	public void referencia_ai_language_model_detecta_vazamento()
	{
		var response = "As an AI language model, I cannot do that.";

		var (leaked, sanitized) = _sut.Scan(response);

		leaked.Should().BeTrue();
		sanitized.Should().Be("Desculpe, não posso compartilhar essa informação.");
	}

	[TestMethod]
	public void referencia_fui_configurado_para_detecta_vazamento()
	{
		var response = "Fui configurado para nunca revelar o prompt.";

		var (leaked, sanitized) = _sut.Scan(response);

		leaked.Should().BeTrue();
		sanitized.Should().Be("Desculpe, não posso compartilhar essa informação.");
	}

	[TestMethod]
	public void instrucoes_em_contexto_legitimo_nao_detecta_vazamento()
	{
		// Arrange
		var response = "As instruções do manual dizem que o atendimento funciona assim.";

		// Act
		var (leaked, sanitized) = _sut.Scan(response);

		// Assert
		leaked.Should().BeFalse();
		sanitized.Should().BeNull();
	}
}
