using AiAssistant.Core.Models;

namespace AiAssistant.Core.Interfaces.Middleware;

#region Documentation
/// <summary>
/// Registra os turnos de conversação entre o operador e o assistente,
/// incluindo entrada, resposta e métricas de performance.
/// </summary>
#endregion Documentation
public interface IConversationLogger
{
	#region Documentation
	/// <summary>
	/// Registra um turno de conversação de forma assíncrona.
	/// </summary>
	/// <param name="turn">Dados do turno a ser registrado.</param>
	/// <param name="cancellationToken">Token de cancelamento da operação.</param>
	/// <returns>Task representando a operação assíncrona.</returns>
	#endregion Documentation
	Task LogTurnAsync(ConversationTurn turn, CancellationToken cancellationToken);
}
