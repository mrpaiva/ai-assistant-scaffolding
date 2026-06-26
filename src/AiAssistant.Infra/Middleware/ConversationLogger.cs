using System.Text.Json;
using Ardalis.GuardClauses;
using AiAssistant.Core.Interfaces.Middleware;
using AiAssistant.Core.Models;
using Microsoft.Extensions.Logging;

namespace AiAssistant.Infra.Middleware;

#region Documentation
/// <summary>
/// Registra turnos de conversação entre o usuário e o assistente via <see cref="ILogger"/>.
/// Os dados são serializados em JSON estruturado para facilitar consultas no sistema de logs.
/// </summary>
/// <remarks>
/// A resposta é truncada a 500 caracteres. Input e resposta são sanitizados para mascarar dados sensíveis.
/// O system prompt nunca é incluído no log.
/// </remarks>
#endregion Documentation
public sealed class ConversationLogger : IConversationLogger
{
	private const int MaxResponseLength = 500;

	private readonly ILogger<ConversationLogger> _logger;

	#region Documentation
	/// <summary>
	/// Cria uma nova instância de <see cref="ConversationLogger"/>.
	/// </summary>
	/// <param name="logger">Logger para registro dos turnos de conversação.</param>
	#endregion Documentation
	public ConversationLogger(ILogger<ConversationLogger> logger)
	{
		Guard.Against.Null(logger);

		_logger = logger;
	}

	#region Documentation
	/// <inheritdoc />
	#endregion Documentation
	public Task LogTurnAsync(ConversationTurn turn, CancellationToken cancellationToken)
	{
		Guard.Against.Null(turn);

		var truncatedResponse = turn.Response.Length > MaxResponseLength
			? turn.Response[..MaxResponseLength] + "..."
			: turn.Response;

		var logEntry = new
		{
			turn.UserId,
			Timestamp = turn.Timestamp.UtcDateTime.ToString("O"),
			Input = LogSanitizer.Sanitize(turn.Input),
			Response = LogSanitizer.Sanitize(truncatedResponse),
			turn.LatencyMs
		};

		var json = JsonSerializer.Serialize(logEntry);

		_logger.LogInformation("ConversationTurn: {TurnData}", json);

		return Task.CompletedTask;
	}
}
