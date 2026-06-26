using System.Text.RegularExpressions;

namespace AiAssistant.Infra.Middleware;

#region Documentation
/// <summary>
/// Mascara dados sensíveis antes de persistência em logs.
/// </summary>
#endregion Documentation
internal static partial class LogSanitizer
{
	#region Documentation
	/// <summary>
	/// Mascara CPF, cartões de crédito e sequências longas de dígitos no texto informado.
	/// </summary>
	/// <param name="text">Texto original.</param>
	/// <returns>Texto com dados sensíveis mascarados.</returns>
	#endregion Documentation
	public static string Sanitize(string? text)
	{
		if (string.IsNullOrEmpty(text)) return string.Empty;

		var sanitized = CpfPattern().Replace(text, "***.***.***-**");
		sanitized = CreditCardPattern().Replace(sanitized, "****-****-****-****");
		sanitized = LongDigitSequencePattern().Replace(sanitized, "[REDACTED]");

		return sanitized;
	}

	[GeneratedRegex(@"\b\d{3}\.?\d{3}\.?\d{3}-?\d{2}\b")]
	private static partial Regex CpfPattern();

	[GeneratedRegex(@"\b(?:\d[ -]*?){13,19}\b")]
	private static partial Regex CreditCardPattern();

	/// <summary>
	/// Mascara sequências de 8+ dígitos (IDs longos, telefones sem formatação). IDs legítimos
	/// de consulta podem ser mascarados intencionalmente — prioridade é evitar vazamento de PII.
	/// </summary>
	[GeneratedRegex(@"\b\d{8,}\b")]
	private static partial Regex LongDigitSequencePattern();
}
