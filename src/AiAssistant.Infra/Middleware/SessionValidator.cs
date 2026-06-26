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
	private readonly ConcurrentDictionary<string, string> _sessionToUser = new();

	#region Documentation
	/// <inheritdoc />
	#endregion Documentation
	public bool RegisterSession(string sessionId, string userId) =>
		_sessionToUser.TryAdd(sessionId, userId);

	#region Documentation
	/// <inheritdoc />
	#endregion Documentation
	public bool Validate(string userId, string sessionId)
	{
		if (!_sessionToUser.TryGetValue(sessionId, out var registeredUser))
			return false;

		return string.Equals(registeredUser, userId, StringComparison.Ordinal);
	}
}
