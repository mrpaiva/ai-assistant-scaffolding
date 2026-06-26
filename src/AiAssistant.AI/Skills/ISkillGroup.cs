using Microsoft.Extensions.AI;

namespace AiAssistant.AI.Skills;

#region Documentation
/// <summary>Grupo de skills com responsabilidade única.</summary>
#endregion Documentation
public interface ISkillGroup
{
	#region Documentation
	/// <summary>Nome do grupo — ex.: "QuerySkills", "ContextSkills".</summary>
	#endregion Documentation
	string GroupName { get; }

	#region Documentation
	/// <summary>
	/// Constrói a lista de AIFunctions do grupo para registro no ChatOptions do agent.
	/// Grupos futuros/não-implementados retornam lista vazia.
	/// </summary>
	#endregion Documentation
	IReadOnlyList<AIFunction> BuildTools();
}
