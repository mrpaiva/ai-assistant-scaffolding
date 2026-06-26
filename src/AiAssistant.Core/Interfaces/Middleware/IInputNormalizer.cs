namespace AiAssistant.Core.Interfaces.Middleware;

#region Documentation
/// <summary>
/// Normaliza a entrada do usuário antes do processamento pelo LLM,
/// removendo caracteres de controle, colapsando espaços e normalizando Unicode.
/// </summary>
#endregion Documentation
public interface IInputNormalizer
{
	#region Documentation
	/// <summary>
	/// Normaliza o texto de entrada do usuário.
	/// </summary>
	/// <param name="input">Texto bruto informado pelo usuário.</param>
	/// <returns>Texto normalizado e seguro para processamento.</returns>
	#endregion Documentation
	string Normalize(string input);
}
