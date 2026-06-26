using System.Text.RegularExpressions;
using AiAssistant.Core.Interfaces.Middleware;

namespace AiAssistant.Infra.Middleware.Security;

#region Documentation
/// <summary>
/// Detecta tentativas de injeção de prompt no texto de entrada do usuário.
/// Identifica padrões em português e inglês e delimitadores de modelos de linguagem.
/// </summary>
/// <remarks>
/// Os padrões são projetados para minimizar falsos positivos em contextos legítimos,
/// como perguntas sobre "instruções de uso do sistema". A detecção é case-insensitive.
/// </remarks>
#endregion Documentation
public sealed partial class InjectionDetector : IInjectionDetector
{
	private static readonly (Regex Pattern, string Description)[] InjectionPatterns =
	[
		// PT-BR — "ignore as instruções" / "ignore todas as instruções" / "ignore as suas instruções"
		// Tolera até 3 palavras entre o verbo e "instruções" (ex.: "todas as suas") — fecha a
		// ressalva do T3 do spike, onde "ignore todas as instruções" não casava o padrão antigo.
		(new Regex(@"ignor[ea]\s+(\w+\s+){0,3}instru[cç][oõ]es", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			"Comando para ignorar instruções (PT-BR)"),

		(new Regex(@"desconsidere\s+(\w+\s+){0,3}instru[cç][oõ]es", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			"Comando para desconsiderar instruções (PT-BR)"),

		(new Regex(@"esque[cç]a\s+tudo", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			"Comando para esquecer contexto (PT-BR)"),

		(new Regex(@"voc[eê]\s+agora\s+[eé]", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			"Tentativa de redefinição de identidade (PT-BR)"),

		// EN — tolera até 3 palavras entre "ignore" e "instructions" (ex.: "all the previous")
		(new Regex(@"ignore\s+(\w+\s+){0,3}instructions", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			"Comando para ignorar instruções (EN)"),

		(new Regex(@"act\s+as\b", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			"Tentativa de redefinição de papel (EN)"),

		(new Regex(@"override\s+instructions", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			"Comando para substituir instruções (EN)"),

		(new Regex(@"you\s+are\s+now", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			"Tentativa de redefinição de identidade (EN)"),

		(new Regex(@"forget\s+everything", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			"Comando para esquecer contexto (EN)"),

		// Delimitadores de sistema e modelos
		(new Regex(@"</system>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			"Delimitador de sistema </system>"),

		(new Regex(@"</instructions>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			"Delimitador de sistema </instructions>"),

		(new Regex(@"\[SYSTEM\]", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			"Delimitador de sistema [SYSTEM]"),

		(new Regex(@"<\|system\|>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			"Delimitador de modelo <|system|>"),

		(new Regex(@"\[/INST\]", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			"Delimitador de modelo [/INST]"),

		(new Regex(@"<<SYS>>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
			"Delimitador de modelo <<SYS>>"),
	];

	#region Documentation
	/// <inheritdoc />
	#endregion Documentation
	public (bool detected, string? pattern) Detect(string input, string operatorId)
	{
		if (string.IsNullOrWhiteSpace(input)) return (false, null);

		foreach (var (pattern, description) in InjectionPatterns)
		{
			if (!pattern.IsMatch(input)) continue;

			return (true, description);
		}

		return (false, null);
	}
}
