using AiAssistant.AI.Agents;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AiAssistant.AI.Hosting;

#region Documentation
/// <summary>
/// Remove periodicamente as sessões expiradas do <see cref="AssistantAgent"/>.
/// </summary>
#endregion Documentation
public sealed class SessionPurgeHostedService : BackgroundService
{
	private static readonly TimeSpan PurgeInterval = TimeSpan.FromMinutes(30);

	private readonly AssistantAgent _agent;
	private readonly ILogger<SessionPurgeHostedService> _logger;

	#region Documentation
	/// <summary>
	/// Cria uma nova instância de <see cref="SessionPurgeHostedService"/>.
	/// </summary>
	/// <param name="agent">Agent MAF do assistente cujas sessões serão purgadas.</param>
	/// <param name="logger">Logger para diagnóstico.</param>
	#endregion Documentation
	public SessionPurgeHostedService(
		AssistantAgent agent,
		ILogger<SessionPurgeHostedService> logger)
	{
		_agent = agent;
		_logger = logger;
	}

	#region Documentation
	/// <inheritdoc />
	#endregion Documentation
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await Task.Delay(PurgeInterval, stoppingToken);

				var removed = _agent.PurgeExpiredSessions();

				if (removed > 0)
					_logger.LogDebug("Removidas {Count} sessões expiradas do Assistant (MAF).", removed);
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception ex)
			{
				#region Comments
				// Mantém o serviço resiliente: qualquer falha transitória na purga é logada e o loop
				// continua, em vez de encerrar o BackgroundService permanentemente.
				#endregion Comments
				_logger.LogError(ex, "Falha ao purgar sessões expiradas do Assistant (MAF).");
			}
		}
	}
}
