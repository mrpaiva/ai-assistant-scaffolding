# AiAssistant — Template .NET 8 para Assistentes com MAF

Template reutilizável para construir assistentes de IA em .NET 8 sobre o **Microsoft Agent Framework (MAF)**, destilado de um projeto real. A arquitetura, as decisões de segurança e as escolhas de design são descritas no artigo [Chatbot com C# e Microsoft Agent Framework (MAF): as decisões que realmente importam](https://medium.com/@mrpaiva/chatbot-com-c-e-microsoft-agent-framework-maf-as-decis%C3%B5es-que-realmente-importam-b6c842a25b80).

---

## Arquitetura

O template é organizado em quatro camadas:

| Camada | Projeto | Responsabilidade |
|---|---|---|
| **Core** | `AiAssistant.Core` | Contratos (`IAssistantAgent`, `ISkillGroup`, middlewares), modelos (`ConversationTurn`, `ClientAction`) e interfaces de infra |
| **Infra** | `AiAssistant.Infra` | Implementações de infra: `ConversationLogger`, `SessionValidator` e middlewares de segurança (`InputNormalizer`, `InjectionDetector`, `OutputScanner`) |
| **AI** | `AiAssistant.AI` | `AssistantAgent`: loop MAF, middleware de segurança, sessão por usuário, purga e `SampleSkillsGroup` |
| **API** | `AiAssistant.API` | Controller REST, DI (`AddAiAssistant`), configuração e UI de chat embutida |

### Fluxo de uma requisição

```
Controller
  → AssistantAgent.ProcessMessageAsync
      → Middleware de segurança MAF:
          1. InputNormalizer  (normaliza: FormKC + remoção de zero-width/homóglifos)
          2. InjectionDetector (detecta → curto-circuita sem chamar o LLM se positivo)
          3. ChatClientAgent  (tool calling automático via ISkillGroups)
          4. OutputScanner    (varre resposta → substitui se vazar instruções internas)
      → ConversationLogger   (registra turno: userId, input, resposta, latência)
  → MessageResponse(Reply, ClientAction?)
```

---

## As decisões que importam

**(1) Skills como grupos componíveis (`ISkillGroup`)**
Cada conjunto de tools é encapsulado em uma classe que implementa `ISkillGroup`. O `AssistantAgent` recebe `IEnumerable<ISkillGroup>` e compõe as tools automaticamente. Novas skills entram sem tocar no agente.

**(2) Segurança como middleware do MAF — 3 barreiras**
As barreiras não são filtros externos ao agente: elas rodam *dentro* do pipeline do MAF, antes e depois do LLM. Isso garante que o bloqueio por guard seja rastreável na auditoria e não confundível com recusa do modelo.

**(3) Sessão por usuário com purga**
O `AssistantAgent` mantém um `ConcurrentDictionary<string, SessionEntry>`. Sessões inativas por mais de 4 h são removidas pela purga periódica. O timeout é injetável para facilitar testes.

**(4) `IChatClient` OpenAI-compat — troca de provedor por configuração**
O agente depende apenas de `IChatClient` (`Microsoft.Extensions.AI`). Para trocar de provedor basta mudar `Assistant:LlmEndpoint` e `Assistant:LlmModel` em `appsettings.json` (ou user-secrets). Sem recompilação.

Essas decisões estão documentadas como ADRs no projeto do Linear.

---

## Quickstart

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)

### 1. Configurar a chave da API

**Nunca commite segredos.** Use user-secrets:

```bash
dotnet user-secrets set "Assistant:LlmApiKey" "<sua-chave>" --project src/AiAssistant.API
```

Você também pode apontar para outro provedor compatível com a API OpenAI (DeepSeek, Groq, Ollama, etc.) alterando em `appsettings.json`:

```json
"Assistant": {
  "LlmEndpoint": "https://api.deepseek.com/v1",
  "LlmModel": "deepseek-chat"
}
```

> O app falha na inicialização se `LlmApiKey` estiver vazio — por design, para evitar erros silenciosos em produção.

#### Usando o OpenRouter

O [OpenRouter](https://openrouter.ai) é um gateway compatível com a API OpenAI — prático e barato. Como o provedor é apenas configuração, funciona sem tocar no código:

```json
"Assistant": {
  "LlmEndpoint": "https://openrouter.ai/api/v1",
  "LlmModel": "openai/gpt-4o-mini"
}
```

```bash
dotnet user-secrets set "Assistant:LlmApiKey" "sk-or-v1-..." --project src/AiAssistant.API
```

**A arquitetura inteira depende do modelo chamar tools** (é assim que as skills funcionam — a `SampleSkillsGroup`, e as suas no futuro):

- **Chat + pipeline de segurança:** funciona com qualquer modelo.
- **Skills (tools):** exige um modelo com suporte a *function calling*. No OpenRouter, escolha um com tools — bons e baratos: `openai/gpt-4o-mini`, `google/gemini-2.0-flash-001`, `deepseek/deepseek-chat`. **Evite** os `:free` (geralmente sem tools e com rate limit agressivo).

Dois detalhes que derrubam quem é novo no OpenRouter:

1. **O `LlmModel` é namespaced** (`openai/gpt-4o-mini`, não `gpt-4o-mini`). Se apontar pro OpenRouter e deixar o default `gpt-4o-mini`, dá erro de modelo inexistente.
2. Os headers `HTTP-Referer`/`X-Title` do OpenRouter são **opcionais** (só ranking/atribuição) — não precisa, o template funciona sem.

### 2. Rodar

```bash
dotnet run --project src/AiAssistant.API
```

Abra o navegador na URL exibida no terminal para acessar a UI de chat.

### 3. Testes

```bash
dotnet test
```

---

## Criando suas skills

1. Implemente `ISkillGroup` em um novo arquivo:

```csharp
public sealed class MinhasSkills : ISkillGroup
{
    public IEnumerable<AITool> BuildTools() =>
        AIFunctionFactory.Create(MinhaFuncao, "minha_funcao", "Descrição da tool").Yield();

    [Description("Executa alguma coisa")]
    private static string MinhaFuncao(string param) => "resultado";
}
```

2. Registre no DI (em `ServiceCollectionExtensions.AddAiAssistant` ou direto em `Program.cs`):

```csharp
services.AddSingleton<ISkillGroup, MinhasSkills>();
```

3. Veja `SampleSkillsGroup` como exemplo de referência — **delete a `SampleSkillsGroup` quando tiver as suas**.

---

## Blocos disponíveis

O `SessionValidator` (em `AiAssistant.Infra`) é um building block testado para validar que um usuário é dono de uma sessão. Registre-o e injete quando adicionar autenticação ao projeto.

---

## Renomear o template

Use `rename.ps1` para substituir o token `AiAssistant` pelo nome do seu projeto em todo o código-fonte, namespaces, arquivos e diretórios:

```powershell
# Dry-run — mostra o que seria feito, sem alterar nada:
./rename.ps1 -Name MeuBot

# Aplicar de verdade:
./rename.ps1 -Name MeuBot -Apply
```

Após o rename, abra a solução pelo novo arquivo `.slnx` gerado.

---

## Segurança

- `appsettings.json` contém apenas placeholders (sem valores reais).
- Segredos vão exclusivamente via `dotnet user-secrets` (desenvolvimento) ou variáveis de ambiente / key vault (produção).
- O `.gitignore` já cobre `appsettings.Development.json`, `secrets.json`, `*.db` e `Data/`.
