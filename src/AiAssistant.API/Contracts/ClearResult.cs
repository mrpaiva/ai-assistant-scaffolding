namespace AiAssistant.API.Contracts;

#region Documentation
/// <summary>
/// Resultado da operação de limpeza de sessão do usuário.
/// </summary>
/// <param name="Cleared">Indica se a sessão foi limpa com sucesso.</param>
#endregion Documentation
public sealed record ClearResult(bool Cleared);
