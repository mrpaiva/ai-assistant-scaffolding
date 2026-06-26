using Ardalis.GuardClauses;
using AiAssistant.AI.Agents;
using AiAssistant.AI.Hosting;
using AiAssistant.AI.Skills;
using AiAssistant.AI.Skills.Sample;
using AiAssistant.Core.Interfaces.AI;
using AiAssistant.Core.Interfaces.Middleware;
using AiAssistant.Core.Models;
using AiAssistant.Infra.Middleware;
using AiAssistant.Infra.Middleware.Security;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using OpenAI;

namespace AiAssistant.API.DI;

#region Documentation
/// <summary>
/// Extensões de registro de serviços do assistente virtual no contêiner de injeção de dependência.
/// </summary>
#endregion Documentation
public static class ServiceCollectionExtensions
{
	#region Documentation
	/// <summary>
	/// Registra todos os serviços necessários para o assistente virtual: cliente LLM (OpenAI-compat),
	/// middlewares de segurança, logger de conversas, grupo de skills de exemplo, agente MAF e
	/// serviço de purga de sessões.
	/// </summary>
	/// <param name="services">Coleção de serviços da aplicação.</param>
	/// <param name="configuration">Configuração da aplicação. Chaves esperadas: <c>Assistant:LlmEndpoint</c>,
	/// <c>Assistant:LlmApiKey</c> (obrigatória), <c>Assistant:LlmModel</c>, <c>Assistant:SessionTimeoutHours</c>.</param>
	/// <returns>A mesma instância de <see cref="IServiceCollection"/> para encadeamento.</returns>
	#endregion Documentation
	public static IServiceCollection AddAiAssistant(
		this IServiceCollection services,
		IConfiguration configuration)
	{
		var llmEndpoint = configuration["Assistant:LlmEndpoint"] ?? "https://api.openai.com/v1";
		var llmApiKey = Guard.Against.NullOrWhiteSpace(
			configuration["Assistant:LlmApiKey"],
			nameof(configuration),
			"A chave de configuração 'Assistant:LlmApiKey' é obrigatória.");
		var llmModel = configuration["Assistant:LlmModel"] ?? "gpt-4o-mini";
		var sessionTimeoutHours = configuration.GetValue("Assistant:SessionTimeoutHours", 4);

		services.AddSingleton<IChatClient>(_ =>
			new OpenAIClient(
				new System.ClientModel.ApiKeyCredential(llmApiKey),
				new OpenAIClientOptions { Endpoint = new Uri(llmEndpoint) })
			.GetChatClient(llmModel)
			.AsIChatClient());

		services.AddSingleton<IInputNormalizer, InputNormalizer>();
		services.AddSingleton<IInjectionDetector, InjectionDetector>();
		services.AddSingleton<IOutputScanner, OutputScanner>();
		services.AddSingleton<IConversationLogger, ConversationLogger>();
		services.AddSingleton<ISessionValidator, SessionValidator>();

		services.AddSingleton<ISkillGroup>(sp =>
		{
			var lazyAgent = new Lazy<AssistantAgent>(() => sp.GetRequiredService<AssistantAgent>());
			return new SampleSkillsGroup(
				() => AssistantAgent.CurrentUserId.Value,
				(userId, action) => lazyAgent.Value.RecordClientAction(userId, action),
				sp.GetRequiredService<ILogger<SampleSkillsGroup>>());
		});

		services.AddSingleton<AssistantAgent>(sp => new AssistantAgent(
			sp.GetRequiredService<IChatClient>(),
			sp.GetServices<ISkillGroup>(),
			sp.GetRequiredService<IInputNormalizer>(),
			sp.GetRequiredService<IInjectionDetector>(),
			sp.GetRequiredService<IOutputScanner>(),
			sp.GetRequiredService<IConversationLogger>(),
			sp.GetRequiredService<ILoggerFactory>(),
			TimeSpan.FromHours(sessionTimeoutHours)));

		services.AddSingleton<IAssistantAgent>(sp => sp.GetRequiredService<AssistantAgent>());
		services.AddHostedService<SessionPurgeHostedService>();

		return services;
	}
}
