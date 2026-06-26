namespace AiAssistant.Core.Interfaces.Middleware;

#region Documentation
/// <summary>
/// Analisa a resposta gerada pelo LLM em busca de vazamento de informações sensíveis,
/// como referências ao prompt de sistema ou instruções internas.
/// </summary>
#endregion Documentation
public interface IOutputScanner
{
	#region Documentation
	/// <summary>
	/// Analisa a resposta do LLM em busca de vazamento de informações sensíveis.
	/// </summary>
	/// <param name="llmResponse">Resposta gerada pelo LLM.</param>
	/// <returns>
	/// Tupla indicando se houve vazamento e, em caso positivo, a resposta sanitizada
	/// que deve substituir a original.
	/// </returns>
	#endregion Documentation
	(bool leaked, string? sanitizedResponse) Scan(string llmResponse);
}
