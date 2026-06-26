using System.Collections.Concurrent;
using AiAssistant.Core.Interfaces.Middleware;

namespace AiAssistant.Infra.Middleware;

#region Documentation
/// <summary>
/// Valida se o operador da requisição corresponde ao operador vinculado à sessão.
/// Mantém um mapeamento in-memory de sessões para operadores usando <see cref="ConcurrentDictionary{TKey,TValue}"/>.
/// </summary>
#endregion Documentation
public sealed class SessionValidator : ISessionValidator
{
	private readonly ConcurrentDictionary<string, string> _sessionToOperator = new();

	#region Documentation
	/// <inheritdoc />
	#endregion Documentation
	public bool RegisterSession(string sessionId, string operatorId) =>
		_sessionToOperator.TryAdd(sessionId, operatorId);

	#region Documentation
	/// <inheritdoc />
	#endregion Documentation
	public bool Validate(string operatorId, string sessionId)
	{
		if (!_sessionToOperator.TryGetValue(sessionId, out var registeredOperator))
			return false;

		return string.Equals(registeredOperator, operatorId, StringComparison.Ordinal);
	}
}
