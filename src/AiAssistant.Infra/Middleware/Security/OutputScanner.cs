using System.Text.RegularExpressions;
using AiAssistant.Core.Interfaces.Middleware;

namespace AiAssistant.Infra.Middleware.Security;

#region Documentation
/// <summary>
/// Analisa a resposta gerada pelo LLM em busca de vazamento de informações sensíveis,
/// como referências ao prompt de sistema ou instruções internas.
/// Quando detecta vazamento, substitui a resposta por uma mensagem padrão.
/// </summary>
#endregion Documentation
public sealed partial class OutputScanner : IOutputScanner
{
	private const string SanitizedMessage = "Desculpe, não posso compartilhar essa informação.";

	#region Documentation
	/// <inheritdoc />
	#endregion Documentation
	public (bool leaked, string? sanitizedResponse) Scan(string llmResponse)
	{
		if (string.IsNullOrWhiteSpace(llmResponse)) return (false, null);

		if (FuiInstruidoPattern().IsMatch(llmResponse)
			|| MinhasInstrucoesPattern().IsMatch(llmResponse)
			|| MeuPromptPattern().IsMatch(llmResponse)
			|| SystemPromptPattern().IsMatch(llmResponse)
			|| IWasToldToPattern().IsMatch(llmResponse)
			|| AiLanguageModelPattern().IsMatch(llmResponse)
			|| MeuPapelEPattern().IsMatch(llmResponse)
			|| FuiConfiguradoParaPattern().IsMatch(llmResponse))
		{
			return (true, SanitizedMessage);
		}

		return (false, null);
	}

	[GeneratedRegex(@"\bfui\s+instru[ií]do\b", RegexOptions.IgnoreCase)]
	private static partial Regex FuiInstruidoPattern();

	[GeneratedRegex(@"\bminhas\s+instru[cç][oõ]es\b", RegexOptions.IgnoreCase)]
	private static partial Regex MinhasInstrucoesPattern();

	[GeneratedRegex(@"\bmeu\s+prompt\b", RegexOptions.IgnoreCase)]
	private static partial Regex MeuPromptPattern();

	[GeneratedRegex(@"\bsystem\s+prompt\b", RegexOptions.IgnoreCase)]
	private static partial Regex SystemPromptPattern();

	[GeneratedRegex(@"\bi\s+was\s+told\s+to\b", RegexOptions.IgnoreCase)]
	private static partial Regex IWasToldToPattern();

	[GeneratedRegex(@"\bas\s+an\s+ai\s+language\s+model\b", RegexOptions.IgnoreCase)]
	private static partial Regex AiLanguageModelPattern();

	[GeneratedRegex(@"\bmeu\s+papel\s+[eé]\b", RegexOptions.IgnoreCase)]
	private static partial Regex MeuPapelEPattern();

	[GeneratedRegex(@"\bfui\s+configurado\s+para\b", RegexOptions.IgnoreCase)]
	private static partial Regex FuiConfiguradoParaPattern();
}
