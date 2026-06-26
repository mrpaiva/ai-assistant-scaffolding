namespace AiAssistant.API.Contracts;

#region Documentation
/// <summary>
/// Informações sobre a sessão ativa de um usuário com o assistente virtual.
/// </summary>
/// <param name="UserId">Identificador único do usuário.</param>
/// <param name="MessageCount">Quantidade de mensagens trocadas na sessão.</param>
/// <param name="CreatedAt">Data e hora de criação da sessão em UTC.</param>
/// <param name="LastActivity">Data e hora da última atividade na sessão em UTC.</param>
#endregion Documentation
public sealed record SessionInfo(string UserId, int MessageCount, DateTime CreatedAt, DateTime LastActivity);
