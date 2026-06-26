using System.Text;
using System.Text.RegularExpressions;
using AiAssistant.Core.Interfaces.Middleware;

namespace AiAssistant.Infra.Middleware.Security;

#region Documentation
/// <summary>
/// Normaliza a entrada do usuário aplicando normalização Unicode FormKC,
/// remoção de caracteres de controle e colapso de espaços em branco múltiplos.
/// </summary>
/// <remarks>
/// A normalização FormKC (Compatibility Decomposition + Canonical Composition) converte
/// homóglifos Unicode para seus equivalentes ASCII, mitigando tentativas de evasão
/// por substituição de caracteres visualmente similares.
/// </remarks>
#endregion Documentation
public sealed partial class InputNormalizer : IInputNormalizer
{
	#region Documentation
	/// <summary>
	/// Normaliza o texto de entrada do usuário.
	/// Remove caracteres de controle (exceto \n, \r, \t), colapsa espaços múltiplos
	/// e aplica normalização Unicode FormKC.
	/// </summary>
	/// <param name="input">Texto bruto informado pelo usuário.</param>
	/// <returns>Texto normalizado e seguro para processamento.</returns>
	#endregion Documentation
	public string Normalize(string input)
	{
		if (string.IsNullOrEmpty(input)) return string.Empty;

		var normalized = input.Normalize(NormalizationForm.FormKC);

		var cleaned = RemoveZeroWidthCharacters(RemoveControlCharacters(normalized));

		cleaned = MultipleSpacesRegex().Replace(cleaned, " ");

		return cleaned.Trim();
	}

	private static string RemoveZeroWidthCharacters(string input)
	{
		var builder = new StringBuilder(input.Length);

		foreach (var character in input)
		{
			if (character is '\u200B' or '\u200C' or '\u200D' or '\uFEFF')
				continue;

			builder.Append(character);
		}

		return builder.ToString();
	}

	private static string RemoveControlCharacters(string input)
	{
		var builder = new StringBuilder(input.Length);

		foreach (var character in input)
		{
			if (char.IsControl(character) && character != '\n' && character != '\r' && character != '\t')
				continue;

			builder.Append(character);
		}

		return builder.ToString();
	}

	[GeneratedRegex(@" {2,}")]
	private static partial Regex MultipleSpacesRegex();
}
