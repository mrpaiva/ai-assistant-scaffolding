using Ardalis.GuardClauses;

namespace AiAssistant.Core.Models;

#region Documentation
/// <summary>
/// Representa um turno de conversação entre o usuário e o assistente,
/// utilizado para fins de auditoria e rastreabilidade.
/// </summary>
#endregion Documentation
public sealed class ConversationTurn
{
	#region Documentation
	/// <summary>Identificador do usuário que originou o turno.</summary>
	#endregion Documentation
	public string UserId { get; }

	#region Documentation
	/// <summary>Momento em que o turno ocorreu.</summary>
	#endregion Documentation
	public DateTimeOffset Timestamp { get; }

	#region Documentation
	/// <summary>Mensagem enviada pelo usuário.</summary>
	#endregion Documentation
	public string Input { get; }

	#region Documentation
	/// <summary>Resposta gerada pelo assistente.</summary>
	#endregion Documentation
	public string Response { get; }

	#region Documentation
	/// <summary>Latência total do processamento, em milissegundos.</summary>
	#endregion Documentation
	public long LatencyMs { get; }

	#region Documentation
	/// <summary>
	/// Inicializa um novo turno de conversação.
	/// </summary>
	/// <param name="userId">Identificador do usuário.</param>
	/// <param name="timestamp">Momento do turno.</param>
	/// <param name="input">Mensagem enviada pelo usuário.</param>
	/// <param name="response">Resposta gerada pelo assistente.</param>
	/// <param name="latencyMs">Latência do processamento em milissegundos.</param>
	#endregion Documentation
	public ConversationTurn(string userId, DateTimeOffset timestamp, string input, string response, long latencyMs)
	{
		UserId = Guard.Against.NullOrWhiteSpace(userId);
		Timestamp = timestamp;
		Input = Guard.Against.NullOrWhiteSpace(input);
		Response = Guard.Against.NullOrWhiteSpace(response);
		LatencyMs = Guard.Against.Negative(latencyMs);
	}
}
