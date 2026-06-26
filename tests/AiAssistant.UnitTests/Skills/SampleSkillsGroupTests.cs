using FluentAssertions;
using AiAssistant.AI.Skills.Sample;
using AiAssistant.Core.Models;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;

namespace AiAssistant.UnitTests.Skills;

#region Documentation
/// <summary>
/// Testes do <see cref="SampleSkillsGroup"/> — verifica que o grupo expõe as tools
/// <c>get_current_time</c> e <c>show_toast</c>, que o toast aciona o callback de
/// <see cref="ClientAction"/> e que a ausência de usuário bloqueia o callback.
/// </summary>
#endregion Documentation
[TestClass]
public sealed class SampleSkillsGroupTests
{
	private static SampleSkillsGroup CriarGrupo(
		Func<string?>? getCurrentUserId = null,
		Action<string, ClientAction>? onClientAction = null) =>
		new(
			getCurrentUserId ?? (() => "user-001"),
			onClientAction ?? ((_, _) => { }),
			NullLogger<SampleSkillsGroup>.Instance);

	[TestMethod]
	public void grupo_de_exemplo_expoe_duas_tools()
	{
		var group = CriarGrupo();

		var tools = group.BuildTools();

		tools.Should().HaveCount(2);
		tools.Select(t => t.Name).Should().BeEquivalentTo(["get_current_time", "show_toast"]);
	}

	[TestMethod]
	public async Task show_toast_aciona_callback_com_clientaction()
	{
		string? userIdCapturado = null;
		ClientAction? actionCapturada = null;

		var group = CriarGrupo(
			getCurrentUserId: () => "user-001",
			onClientAction: (userId, action) =>
			{
				userIdCapturado = userId;
				actionCapturada = action;
			});

		var tool = group.BuildTools().Single(t => t.Name == "show_toast");

		var resultado = await tool.InvokeAsync(
			new AIFunctionArguments(new Dictionary<string, object?> { ["message"] = "olá" }));

		userIdCapturado.Should().Be("user-001");
		actionCapturada.Should().NotBeNull();
		actionCapturada!.Type.Should().Be("showToast");
		actionCapturada.Payload.Should().Be("olá");
		resultado?.ToString().Should().Contain("olá");
	}

	[TestMethod]
	public async Task show_toast_sem_usuario_nao_aciona_callback()
	{
		var callbackAcionado = false;

		var group = CriarGrupo(
			getCurrentUserId: () => null,
			onClientAction: (_, _) => callbackAcionado = true);

		var tool = group.BuildTools().Single(t => t.Name == "show_toast");

		var resultado = await tool.InvokeAsync(
			new AIFunctionArguments(new Dictionary<string, object?> { ["message"] = "olá" }));

		callbackAcionado.Should().BeFalse();
		resultado?.ToString().Should().Contain("não foi possível identificar");
	}

	[TestMethod]
	public async Task get_current_time_retorna_texto_nao_vazio()
	{
		var group = CriarGrupo();

		var tool = group.BuildTools().Single(t => t.Name == "get_current_time");

		var resultado = await tool.InvokeAsync(new AIFunctionArguments());

		resultado?.ToString().Should().NotBeNullOrWhiteSpace();
	}
}
