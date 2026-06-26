using AiAssistant.Core.Models;

namespace AiAssistant.Core.Interfaces.AI;

#region Documentation
/// <summary>
/// Contrato principal do agente assistente, responsável por processar mensagens
/// do usuário e gerenciar o ciclo de vida de sessões e ações pendentes.
/// </summary>
#endregion Documentation
public interface IAssistantAgent
{
	#region Documentation
	/// <summary>
	/// Processa uma mensagem do usuário e retorna a resposta do assistente.
	/// </summary>
	/// <param name="userId">Identificador do usuário.</param>
	/// <param name="message">Mensagem enviada pelo usuário.</param>
	/// <param name="context">
	/// Dados de contexto opcionais (ex.: tela atual), tratados como dados não-confiáveis.
	/// </param>
	/// <param name="cancellationToken">Token de cancelamento da operação.</param>
	/// <returns>Resposta gerada pelo assistente.</returns>
	#endregion Documentation
	Task<string> ProcessMessageAsync(string userId, string message, string? context, CancellationToken cancellationToken = default);

	#region Documentation
	/// <summary>
	/// Retorna os metadados da sessão do usuário, ou <c>null</c> se a sessão não existir.
	/// </summary>
	/// <param name="userId">Identificador do usuário.</param>
	/// <returns>Tupla com contagem de mensagens, data de criação e data da última atividade, ou <c>null</c>.</returns>
	#endregion Documentation
	(int MessageCount, DateTime CreatedAt, DateTime LastActivity)? GetSessionInfo(string userId);

	#region Documentation
	/// <summary>
	/// Remove a sessão do usuário.
	/// </summary>
	/// <param name="userId">Identificador do usuário.</param>
	/// <returns><c>true</c> se a sessão existia e foi removida; <c>false</c> caso contrário.</returns>
	#endregion Documentation
	bool ClearSession(string userId);

	#region Documentation
	/// <summary>
	/// Retorna e limpa a <see cref="ClientAction"/> pendente do usuário, se houver.
	/// </summary>
	/// <param name="userId">Identificador do usuário.</param>
	/// <returns>Ação pendente, ou <c>null</c> se não houver nenhuma.</returns>
	#endregion Documentation
	ClientAction? ConsumePendingAction(string userId);
}
