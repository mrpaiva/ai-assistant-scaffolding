using System.ComponentModel;
using Ardalis.GuardClauses;
using AiAssistant.Core.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace AiAssistant.AI.Skills.Sample;

#region Documentation
/// <summary>
/// <b>Skill de exemplo / template — delete-a quando criar suas próprias skills.</b>
/// </summary>
/// <remarks>
/// <para>
/// Este grupo existe exclusivamente para demonstrar o padrão de plugin de skills do
/// <c>AiAssistant</c>. Ele contém duas tools intencionalmente triviais:
/// </para>
/// <list type="number">
///   <item>
///     <term><c>get_current_time</c></term>
///     <description>Tool pura — sem efeitos colaterais. Retorna a data e hora atuais.</description>
///   </item>
///   <item>
///     <term><c>show_toast</c></term>
///     <description>
///       Tool com efeito colateral no client — emite uma <see cref="ClientAction"/> via callback,
///       demonstrando como o agente se comunica com o front-end sem retornar JSON na resposta.
///     </description>
///   </item>
/// </list>
/// <para>
/// Quando criar suas próprias skills, remova esta classe e o namespace
/// <c>AiAssistant.AI.Skills.Sample</c> inteiramente.
/// </para>
/// </remarks>
#endregion Documentation
public sealed class SampleSkillsGroup : ISkillGroup
{
	private readonly Func<string?> _getCurrentUserId;
	private readonly Action<string, ClientAction> _onClientAction;
	private readonly ILogger<SampleSkillsGroup> _logger;

	#region Documentation
	/// <summary>
	/// Cria uma nova instância de <see cref="SampleSkillsGroup"/>.
	/// </summary>
	/// <param name="getCurrentUserId">Delegate que retorna o id do usuário corrente (pode retornar <c>null</c> se não autenticado).</param>
	/// <param name="onClientAction">Callback invocado com o id do usuário e a <see cref="ClientAction"/> quando uma tool precisa agir no client.</param>
	/// <param name="logger">Logger de diagnóstico.</param>
	#endregion Documentation
	public SampleSkillsGroup(
		Func<string?> getCurrentUserId,
		Action<string, ClientAction> onClientAction,
		ILogger<SampleSkillsGroup> logger)
	{
		Guard.Against.Null(getCurrentUserId);
		Guard.Against.Null(onClientAction);
		Guard.Against.Null(logger);

		_getCurrentUserId = getCurrentUserId;
		_onClientAction = onClientAction;
		_logger = logger;
	}

	#region Documentation
	/// <inheritdoc />
	#endregion Documentation
	public string GroupName => "SampleSkills";

	#region Documentation
	/// <inheritdoc />
	#endregion Documentation
	public IReadOnlyList<AIFunction> BuildTools() =>
	[
		AIFunctionFactory.Create(
			GetCurrentTime,
			"get_current_time",
			"Retorna a data e hora atuais. Use quando o usuário perguntar que horas são ou a data de hoje."),
		AIFunctionFactory.Create(
			ShowToast,
			"show_toast",
			"Exibe uma notificação (toast) ao usuário com a mensagem informada.")
	];

	#region Documentation
	/// <summary>
	/// Retorna a data e hora atuais formatadas. Tool pura — sem efeitos colaterais.
	/// </summary>
	#endregion Documentation
	private static string GetCurrentTime() =>
		DateTimeOffset.Now.ToString("F");

	#region Documentation
	/// <summary>
	/// Exibe uma notificação ao usuário via <see cref="ClientAction"/>.
	/// Só aciona o callback quando há um usuário identificado na sessão.
	/// </summary>
	/// <param name="message">Mensagem a exibir no toast.</param>
	#endregion Documentation
	private string ShowToast([Description("Mensagem a exibir no toast")] string message)
	{
		var userId = _getCurrentUserId();

		if (string.IsNullOrWhiteSpace(userId))
		{
			_logger.LogWarning("show_toast chamado sem usuário identificado — callback suprimido.");
			return "não foi possível identificar o usuário. O toast não foi exibido.";
		}

		_onClientAction(userId, new ClientAction("showToast", message));
		_logger.LogInformation("Toast enviado ao usuário {UserId}: {Message}", userId, message);

		return $"Toast exibido: {message}";
	}
}
