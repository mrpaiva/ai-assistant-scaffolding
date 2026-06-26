using AiAssistant.Core.Models;

namespace AiAssistant.API.Contracts;

#region Documentation
/// <summary>
/// Resposta do assistente virtual a uma mensagem do usuário.
/// </summary>
/// <param name="Reply">Texto da resposta gerada pelo assistente.</param>
/// <param name="Action">Ação a executar no client após a resposta, quando houver.</param>
#endregion Documentation
public sealed record MessageResponse(string Reply, ClientAction? Action = null);
