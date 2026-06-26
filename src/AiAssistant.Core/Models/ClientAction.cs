namespace AiAssistant.Core.Models;

#region Documentation
/// <summary>Ação a executar no client após a resposta do assistente (ex.: navegar, exibir toast, abrir modal).</summary>
/// <param name="Type">Tipo da ação, interpretado pelo client (ex.: "showToast", "navigate").</param>
/// <param name="Payload">Dados opcionais da ação.</param>
#endregion Documentation
public sealed record ClientAction(string Type, string? Payload = null);
