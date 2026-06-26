namespace AiAssistant.Core.Interfaces.Middleware;

#region Documentation
/// <summary>
/// Detecta tentativas de injeção de prompt no texto de entrada do usuário.
/// Identifica padrões em português e inglês, além de delimitadores de sistema.
/// </summary>
#endregion Documentation
public interface IInjectionDetector
{
	#region Documentation
	/// <summary>
	/// Analisa o texto de entrada em busca de padrões de injeção de prompt.
	/// </summary>
	/// <param name="input">Texto de entrada do usuário.</param>
	/// <param name="userId">Identificador do operador para controle de rate limit.</param>
	/// <returns>
	/// Tupla indicando se uma tentativa de injeção foi detectada e,
	/// em caso positivo, o padrão identificado.
	/// </returns>
	#endregion Documentation
	(bool detected, string? pattern) Detect(string input, string userId);
}
