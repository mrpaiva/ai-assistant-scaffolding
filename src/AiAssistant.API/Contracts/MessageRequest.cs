namespace AiAssistant.API.Contracts;

#region Documentation
/// <summary>
/// Requisição de envio de mensagem ao assistente virtual.
/// </summary>
/// <param name="UserId">Identificador único do usuário que enviou a mensagem.</param>
/// <param name="Message">Texto da mensagem enviada pelo usuário.</param>
/// <param name="Context">
/// Contexto opcional da interação (ex.: tela atual, dados relevantes).
/// Pode ser nulo quando o contexto não estiver disponível.
/// </param>
#endregion Documentation
public sealed record MessageRequest(string UserId, string Message, string? Context = null);
