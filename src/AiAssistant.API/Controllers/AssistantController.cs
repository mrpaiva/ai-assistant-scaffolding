using Ardalis.GuardClauses;
using AiAssistant.API.Contracts;
using AiAssistant.Core.Interfaces.AI;
using Microsoft.AspNetCore.Mvc;

namespace AiAssistant.API.Controllers;

#region Documentation
/// <summary>
/// Controller REST do assistente virtual. Expõe endpoints de chat, consulta e limpeza de sessão.
/// </summary>
#endregion Documentation
[ApiController]
[Route("api/assistant")]
public sealed class AssistantController : ControllerBase
{
	private readonly IAssistantAgent _agent;

	#region Documentation
	/// <summary>
	/// Cria uma nova instância de <see cref="AssistantController"/>.
	/// </summary>
	/// <param name="agent">Agente assistente responsável pelo processamento das mensagens.</param>
	#endregion Documentation
	public AssistantController(IAssistantAgent agent)
	{
		Guard.Against.Null(agent);
		_agent = agent;
	}

	#region Documentation
	/// <summary>
	/// Envia uma mensagem ao assistente e retorna a resposta junto de qualquer ação pendente no client.
	/// </summary>
	/// <param name="request">Dados da mensagem: identificador do usuário, texto e contexto opcional.</param>
	/// <param name="cancellationToken">Token de cancelamento da operação.</param>
	/// <returns>
	/// <see cref="MessageResponse"/> com o texto da resposta e, quando houver, uma <c>ClientAction</c>.
	/// Retorna <c>400 Bad Request</c> se o identificador ou a mensagem estiverem em branco.
	/// </returns>
	#endregion Documentation
	[HttpPost("message")]
	public async Task<IActionResult> SendMessageAsync(
		[FromBody] MessageRequest request,
		CancellationToken cancellationToken)
	{
		if (string.IsNullOrWhiteSpace(request.UserId))
			return BadRequest(new { error = "O identificador do usuário é obrigatório." });

		if (string.IsNullOrWhiteSpace(request.Message))
			return BadRequest(new { error = "A mensagem é obrigatória." });

		var reply = await _agent.ProcessMessageAsync(request.UserId, request.Message, request.Context, cancellationToken);
		var action = _agent.ConsumePendingAction(request.UserId);

		return Ok(new MessageResponse(reply, action));
	}

	#region Documentation
	/// <summary>
	/// Retorna as informações da sessão ativa do usuário.
	/// </summary>
	/// <param name="userId">Identificador do usuário.</param>
	/// <returns>
	/// <see cref="SessionInfo"/> com os dados da sessão, ou <c>404 Not Found</c> se não houver sessão ativa.
	/// Retorna <c>400 Bad Request</c> se o identificador estiver em branco.
	/// </returns>
	#endregion Documentation
	[HttpGet("session/{userId}")]
	public IActionResult GetSession(string userId)
	{
		if (string.IsNullOrWhiteSpace(userId))
			return BadRequest(new { error = "O identificador do usuário é obrigatório." });

		var info = _agent.GetSessionInfo(userId);

		if (info is null)
			return NotFound(new { error = "Nenhuma sessão ativa encontrada para este usuário." });

		var (messageCount, createdAt, lastActivity) = info.Value;

		return Ok(new SessionInfo(userId, messageCount, createdAt, lastActivity));
	}

	#region Documentation
	/// <summary>
	/// Remove a sessão do usuário.
	/// </summary>
	/// <param name="userId">Identificador do usuário.</param>
	/// <returns>
	/// <see cref="ClearResult"/> com <c>Cleared = true</c>.
	/// Retorna <c>400 Bad Request</c> se o identificador estiver em branco.
	/// </returns>
	#endregion Documentation
	[HttpDelete("session/{userId}")]
	public IActionResult ClearSession(string userId)
	{
		if (string.IsNullOrWhiteSpace(userId))
			return BadRequest(new { error = "O identificador do usuário é obrigatório." });

		_agent.ClearSession(userId);

		return Ok(new ClearResult(true));
	}
}
