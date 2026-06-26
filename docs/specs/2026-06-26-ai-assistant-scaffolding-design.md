# Design — ai-assistant-scaffolding

**Data:** 2026-06-26
**Origem:** destilado do projeto `LittleMarcos` (branch `user/mauro/little-marcos`,
`Applications/LittleMarcos`), o chatbot C# + Microsoft Agent Framework descrito no artigo
[*Chatbot com C# e Microsoft Agent Framework (MAF): as decisões que realmente importam*](https://medium.com/@mrpaiva/chatbot-com-c-e-microsoft-agent-framework-maf-as-decis%C3%B5es-que-realmente-importam-b6c842a25b80).
**Destino:** `E:\source\ai-assistant-scaffolding` (repositório-template GitHub standalone).

## Problema

Cada novo projeto de assistente de IA recomeça do zero a mesma fundação: agent sobre o MAF,
sessão por usuário, pipeline de defesa contra prompt injection, contrato de skills, fiação de
DI e uma UI de chat. O `LittleMarcos` já resolveu tudo isso bem, mas está soldado ao domínio
MultiClubes (text-to-SQL, glossário, ADO). Queremos extrair **só a fundação reutilizável** para
um template do qual qualquer assistente novo possa nascer com um comando.

## Decisões travadas (brainstorming 2026-06-26)

1. **Escopo:** esqueleto + **uma** skill-exemplo trivial. Zero domínio MultiClubes.
2. **Provedor LLM:** OpenAI-compat genérico (`IChatClient` via `OpenAIClient` com
   `Endpoint`+`ApiKey`+`Model` configuráveis). Funciona com OpenAI, DeepSeek, Groq, Ollama-compat.
3. **Mecanismo de reúso:** repositório-template GitHub, namespace neutro `AiAssistant`,
   script `rename.ps1` opcional.
4. **ClientAction:** mantido e generalizado no core (ex-`NavigationCommand`). A skill-exemplo
   dispara uma `ClientAction` para demonstrar tool com efeito colateral surfado ao client.
5. **Context:** parâmetro `string? context` opcional no agent, isolado por nonce (barreira 2).
6. **Testes:** portar os testes unitários genéricos (segurança, sessão, agent, skill-exemplo);
   base nasce com suite verde.

## Não-objetivos (o que sai do LittleMarcos)

- Text-to-SQL: `QuerySkill`, `SchemaRegistryBuilder`, `SchemaRepository`, `SelectOnlyGuard`,
  `TopNInjector`, `TimeoutEnforcer`/`TimeoutCancellation`, `SqlQueryExecutor`, few-shot RAG
  (`FewShotRepository`/`FewShotIndexBuilder` + SQLite).
- Integração externa: `AdoRepositoryClient`, `ExplainerSkillsGroup`, `GlossaryEnrichmentService`.
- Skills de navegação específicas: `OperationSkillsGroup`, `ReportSkillsGroup`,
  `NotificationSkillsGroup`, `ContextSkillsGroup` (e `capability-catalog.json`).
- **Segredos commitados** do `appsettings.json` (chave DeepSeek, senha `sa`). A base usa
  placeholders + `dotnet user-secrets`.

## Arquitetura

Quatro camadas (idêntico ao LittleMarcos, renomeado) + testes:

```
ai-assistant-scaffolding/
├─ README.md                      # arquitetura + link do artigo + quickstart
├─ rename.ps1                     # AiAssistant -> NomeDoSeuBot (namespaces/pastas/sln)
├─ .gitignore  .editorconfig
├─ Directory.Build.props          # net8.0, ImplicitUsings, Nullable, LangVersion
├─ Directory.Packages.props       # versões NuGet centralizadas (Central Package Management)
├─ AiAssistant.slnx
├─ docs/
│   ├─ specs/2026-06-26-ai-assistant-scaffolding-design.md   # este documento
│   └─ adr/
│       ├─ 0001-skills-como-grupos-componiveis.md            # ex-ADR-005
│       └─ 0002-seguranca-como-middleware-do-maf.md          # ex-ADR-004
├─ src/
│   ├─ AiAssistant.Core/
│   │   ├─ Interfaces/AI/IAssistantAgent.cs
│   │   ├─ Interfaces/Middleware/{IInputNormalizer,IInjectionDetector,IOutputScanner,
│   │   │                          ISessionValidator,IConversationLogger}.cs
│   │   ├─ Interfaces/Skills/ (vazio inicialmente — ponto de extensão)
│   │   └─ Models/{ConversationTurn,ClientAction}.cs
│   ├─ AiAssistant.Infra/
│   │   ├─ Middleware/Security/{InputNormalizer,InjectionDetector,OutputScanner}.cs
│   │   ├─ Middleware/{SessionValidator,ConversationLogger,LogSanitizer}.cs
│   ├─ AiAssistant.AI/
│   │   ├─ Agents/AssistantAgent.cs            # MAF: ChatClientAgent + AIAgentBuilder.Use
│   │   ├─ Skills/ISkillGroup.cs
│   │   ├─ Skills/Sample/SampleSkillsGroup.cs  # 1 tool trivial + 1 ClientAction de exemplo
│   │   ├─ Hosting/SessionPurgeHostedService.cs
│   │   └─ Prompts/assistant-system.md         # EmbeddedResource
│   └─ AiAssistant.API/
│       ├─ Program.cs
│       ├─ DI/ServiceCollectionExtensions.cs   # AddAiAssistant(configuration)
│       ├─ Controllers/AssistantController.cs   # POST /api/assistant/message, GET/DELETE session
│       ├─ Contracts/{MessageRequest,MessageResponse,SessionInfo,ClearResult}.cs
│       ├─ appsettings.json                     # placeholders, sem segredos
│       └─ wwwroot/{index.html,style.css,assets/}  # chat web rebrandeado
└─ tests/
    └─ AiAssistant.UnitTests/
        ├─ Security/{InputNormalizer,InjectionDetector,OutputScanner,InjectionPipeline}Tests.cs
        ├─ Session/SessionValidatorTests.cs
        ├─ Infra/ConversationLoggerTests.cs
        ├─ Agent/AssistantAgentTests.cs
        └─ Skills/SampleSkillsGroupTests.cs
```

### Componentes e contratos

**`IAssistantAgent` (Core)** — superfície mínima do agent:
- `Task<string> ProcessMessageAsync(string userId, string message, string? context, CancellationToken ct)`
- `(int MessageCount, DateTime CreatedAt, DateTime LastActivity)? GetSessionInfo(string userId)`
- `bool ClearSession(string userId)`
- `ClientAction? ConsumePendingAction(string userId)`

**`AssistantAgent` (AI)** — implementação MAF:
- Constrói `ChatClientAgent` com `Instructions` (prompt embutido) + `Tools` agregadas de
  `IEnumerable<ISkillGroup>`.
- Embrulha com `AIAgentBuilder.Use(RunWithSecurityMiddlewareAsync)`: barreira 1 normaliza+detecta
  injeção e curto-circuita **sem chamar o LLM**; barreira 3 varre a saída.
- Sessão por `userId` em `ConcurrentDictionary`, expiração por inatividade (default 4h, parametrizável).
- `AsyncLocal<string?>` para propagar `userId` às skills sem passá-lo por parâmetro de tool.
- `BuildInput` isola `message` e `context` por nonce (barreira 2).

**`ISkillGroup` (AI)** — `string GroupName { get; }` + `IReadOnlyList<AIFunction> BuildTools()`.
Ponto de extensão central: cada capacidade nova é um grupo registrado no DI.

**`SampleSkillsGroup` (AI)** — exemplo deletável: uma tool `GetCurrentTime` (texto) e uma tool
que registra uma `ClientAction` de exemplo (ex.: `{ Type: "showToast", Payload: "..." }`)
consumida na resposta do controller.

**Segurança (Infra)** — portados sem alteração de lógica, só de namespace:
- `InputNormalizer`: FormKC + remoção de control/zero-width + colapso de espaços.
- `InjectionDetector`: padrões regex PT-BR/EN + delimitadores de modelo.
- `OutputScanner`: detecção de vazamento de prompt/instruções, substitui por mensagem sanitizada.
- `SessionValidator`, `ConversationLogger` + `LogSanitizer` (auditoria por turno).

**API** — `AssistantController` (`POST /message`, `GET /session/{userId}`, `DELETE /session/{userId}`),
`Program.cs` minimal hosting, `wwwroot` chat com `marked` + `DOMPurify` (CDN pinada).

### Fluxo de uma mensagem
```
POST /api/assistant/message {userId, message, context?}
  → AssistantController
  → AssistantAgent.ProcessMessageAsync
      → middleware: InputNormalizer → InjectionDetector (curto-circuita se injeção)
      → BuildInput (isola message+context por nonce)
      → ChatClientAgent.RunAsync (LLM + tool-calling automático sobre ISkillGroups)
      → middleware: OutputScanner (mascara vazamento)
      → ConversationLogger.LogTurnAsync
  → ConsumePendingAction → MessageResponse {reply, action?}
```

### Configuração (`appsettings.json`, sem segredos)
```json
{
  "Assistant": {
    "LlmEndpoint": "https://api.openai.com/v1",
    "LlmApiKey": "",
    "LlmModel": "gpt-4o-mini",
    "SessionTimeoutHours": 4
  }
}
```
Segredo real via `dotnet user-secrets set "Assistant:LlmApiKey" "..."`. `Guard.Against.NullOrWhiteSpace`
na chave falha cedo com mensagem clara.

## Stack
- .NET 8.0, `ImplicitUsings`, `Nullable` enable.
- `Microsoft.Agents.AI` (MAF) + `Microsoft.Extensions.AI` (+ `.OpenAI`).
- `Ardalis.GuardClauses`.
- Testes: MSTest + FluentAssertions + NSubstitute.
- Central Package Management (`Directory.Packages.props`).

## Tratamento de erros
- Exceção no processamento → resposta amigável genérica + log de erro (paridade com o agent original).
- Injeção detectada → resposta canônica fixa do guard, sem LLM (evidencia bloqueio por middleware).
- Vazamento na saída → mensagem sanitizada do scanner.
- Config de LLM ausente → falha no startup via guard (não em runtime).

## Estratégia de testes
Portar os testes genéricos do LittleMarcos (renomeados): unidade dos três middlewares de
segurança, do pipeline de injeção, do `SessionValidator`, do `ConversationLogger`, do agent
(`AssistantAgent`) com `IChatClient` mockado, e do `SampleSkillsGroup`. Meta: `dotnet test`
verde no clone limpo (depois de configurar a chave). Critério de pronto do scaffolding.

## Riscos / cuidados
- **Segredos:** garantir que NENHUM segredo do LittleMarcos vaze pro template; `.gitignore`
  cobre `appsettings.Development.json` e `secrets.json`.
- **Versões MAF:** o MAF é pré-1.0/move rápido; fixar versões em `Directory.Packages.props`.
- **`rename.ps1`:** precisa cobrir nomes de pasta, namespaces, `.csproj`, `.slnx`,
  `InternalsVisibleTo`, nome do prompt embutido e o `resourceName` em `LoadEmbeddedPrompt`.

## Próximos passos
Após aprovação deste design: `writing-plans` para o plano de implementação detalhado
(ordem: scaffold da solução → portar Core/Infra de segurança → AssistantAgent → SampleSkill →
API/UI → testes → README/ADRs/rename.ps1 → `dotnet test` verde → git init + template).
