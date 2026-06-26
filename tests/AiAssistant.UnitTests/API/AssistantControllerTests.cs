using AiAssistant.API.Contracts;
using AiAssistant.API.Controllers;
using AiAssistant.Core.Interfaces.AI;
using AiAssistant.Core.Models;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace AiAssistant.UnitTests.API;

#region Documentation
/// <summary>
/// Testes unitários do <see cref="AssistantController"/>, validando mapeamento de
/// entradas inválidas para BadRequest, sessão inexistente para NotFound e resposta
/// completa (reply + action) para requisição válida.
/// </summary>
#endregion Documentation
[TestClass]
public class AssistantControllerTests
{
	private IAssistantAgent _agent = null!;
	private AssistantController _sut = null!;

	[TestInitialize]
	public void Setup()
	{
		_agent = Substitute.For<IAssistantAgent>();
		_sut = new AssistantController(_agent);
	}

	[TestMethod]
	public async Task mensagem_vazia_retorna_badrequest()
	{
		var request = new MessageRequest("user1", string.Empty);

		var result = await _sut.SendMessageAsync(request, CancellationToken.None);

		result.Should().BeOfType<BadRequestObjectResult>();
	}

	[TestMethod]
	public async Task userid_vazio_retorna_badrequest()
	{
		var request = new MessageRequest(string.Empty, "Olá");

		var result = await _sut.SendMessageAsync(request, CancellationToken.None);

		result.Should().BeOfType<BadRequestObjectResult>();
	}

	[TestMethod]
	public async Task mensagem_valida_retorna_ok_com_resposta_e_action()
	{
		var expectedAction = new ClientAction("showToast", "x");
		_agent.ProcessMessageAsync("user1", "Oi", null, Arg.Any<CancellationToken>()).Returns("oi");
		_agent.ConsumePendingAction("user1").Returns(expectedAction);

		var request = new MessageRequest("user1", "Oi");

		var result = await _sut.SendMessageAsync(request, CancellationToken.None);

		var ok = result.Should().BeOfType<OkObjectResult>().Subject;
		var response = ok.Value.Should().BeOfType<MessageResponse>().Subject;
		response.Reply.Should().Be("oi");
		response.Action.Should().NotBeNull();
		response.Action!.Type.Should().Be("showToast");
	}

	[TestMethod]
	public void sessao_inexistente_retorna_notfound()
	{
		_agent.GetSessionInfo("user42").Returns((ValueTuple<int, DateTime, DateTime>?)null);

		var result = _sut.GetSession("user42");

		result.Should().BeOfType<NotFoundObjectResult>();
	}
}
