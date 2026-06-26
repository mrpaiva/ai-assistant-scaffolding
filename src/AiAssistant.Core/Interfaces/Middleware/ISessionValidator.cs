namespace AiAssistant.Core.Interfaces.Middleware;

#region Documentation
/// <summary>
/// Valida se o operador da requisição corresponde ao operador vinculado à sessão,
/// impedindo que um operador acesse a sessão de outro.
/// </summary>
#endregion Documentation
public interface ISessionValidator
{
	#region Documentation
	/// <summary>
	/// Registra o vínculo entre uma sessão e um operador.
	/// </summary>
	/// <param name="sessionId">Identificador da sessão.</param>
	/// <param name="userId">Identificador do operador vinculado.</param>
	/// <returns><c>true</c> se o registro foi criado; <c>false</c> se a sessão já existia.</returns>
	#endregion Documentation
	bool RegisterSession(string sessionId, string userId);

	#region Documentation
	/// <summary>
	/// Valida se o operador informado corresponde ao operador da sessão.
	/// </summary>
	/// <param name="userId">Identificador do operador da requisição.</param>
	/// <param name="sessionId">Identificador da sessão.</param>
	/// <returns><c>true</c> se o operador corresponde à sessão; <c>false</c> caso contrário.</returns>
	#endregion Documentation
	bool Validate(string userId, string sessionId);
}
