using FluentAssertions;
using AiAssistant.Infra.Middleware.Security;

namespace AiAssistant.UnitTests.Security;

[TestClass]
public class InputNormalizerTests
{
	private InputNormalizer _sut = null!;

	[TestInitialize]
	public void Setup()
	{
		_sut = new InputNormalizer();
	}

	[TestMethod]
	public void homoglifo_unicode_normalizado_para_forma_ascii_equivalente()
	{
		// Arrange — caractere Unicode "ⅰ" (U+2170, Roman Numeral Small One)
		var input = "ⅰgnore";

		// Act
		var result = _sut.Normalize(input);

		// Assert — FormKC normaliza "ⅰ" para "i"
		result.Should().Be("ignore");
	}

	[TestMethod]
	public void multiplos_espacos_colapsados_em_um_unico()
	{
		// Arrange
		var input = "quais   pagamentos    vencem   essa   semana";

		// Act
		var result = _sut.Normalize(input);

		// Assert
		result.Should().Be("quais pagamentos vencem essa semana");
	}

	[TestMethod]
	public void caracteres_de_controle_removidos()
	{
		// Arrange — \0 (null) e \x07 (bell) são caracteres de controle
		var input = "teste" + '\0' + "com" + '\x07' + "caracteres";

		// Act
		var result = _sut.Normalize(input);

		// Assert
		result.Should().Be("testecomcaracteres");
	}

	[TestMethod]
	public void caracteres_de_controle_permitidos_preservados()
	{
		// Arrange
		var input = "linha1\nlinha2\rlinha3\ttabulado";

		// Act
		var result = _sut.Normalize(input);

		// Assert
		result.Should().Be("linha1\nlinha2\rlinha3\ttabulado");
	}

	[TestMethod]
	public void input_limpo_permanece_inalterado()
	{
		// Arrange
		var input = "quais pagamentos vencem essa semana?";

		// Act
		var result = _sut.Normalize(input);

		// Assert
		result.Should().Be("quais pagamentos vencem essa semana?");
	}

	[TestMethod]
	public void espacos_no_inicio_e_fim_removidos()
	{
		// Arrange
		var input = "  texto com espacos  ";

		// Act
		var result = _sut.Normalize(input);

		// Assert
		result.Should().Be("texto com espacos");
	}
}
