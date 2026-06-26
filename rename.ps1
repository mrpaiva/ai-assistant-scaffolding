#Requires -Version 7.0
<#
.SYNOPSIS
    Renomeia o template AiAssistant para um novo nome de projeto.

.DESCRIPTION
    Substitui o token 'AiAssistant' pelo valor de -Name em conteúdo de arquivos
    (.cs, .csproj, .slnx, .json, .props, .md, .html, .editorconfig, .ps1) e
    renomeia arquivos e diretórios cujos nomes contenham 'AiAssistant'.

    Por padrão executa um DRY-RUN e imprime o que seria feito sem alterar nada.
    Passe -Apply para efetivar as mudanças.

.PARAMETER Name
    Novo nome do projeto. Deve ser um identificador C# válido (letras/dígitos,
    começa com letra, sem espaços ou pontos).

.PARAMETER Apply
    Efetiva as alterações. Sem este switch o script executa em modo dry-run.

.EXAMPLE
    ./rename.ps1 -Name MeuBot
    ./rename.ps1 -Name MeuBot -Apply
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Name,

    [switch]$Apply
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# --------------------------------------------------------------------------
# Validação do nome
# --------------------------------------------------------------------------
if ($Name -notmatch '^[A-Za-z][A-Za-z0-9]*$') {
    Write-Error "Nome inválido: '$Name'. Use apenas letras e dígitos, começando com uma letra (sem espaços, pontos ou outros caracteres)."
    exit 1
}

if ($Name -eq 'AiAssistant') {
    Write-Error "O nome deve ser diferente do token original 'AiAssistant'."
    exit 1
}

# --------------------------------------------------------------------------
# Configuração
# --------------------------------------------------------------------------
$token       = 'AiAssistant'
$scriptFile  = $MyInvocation.MyCommand.Path   # este arquivo — pular edição de conteúdo

$extensions = @('.cs', '.csproj', '.slnx', '.json', '.props', '.md', '.html', '.editorconfig', '.ps1')

$excludeDirs = @('bin', 'obj', '.git', '.vs', 'docs', '.superpowers')

$root = $PSScriptRoot
if (-not $root) { $root = (Get-Location).Path }

$mode = if ($Apply) { 'APPLY' } else { 'DRY-RUN' }
Write-Host ""
Write-Host "=== rename.ps1 — modo: $mode ===" -ForegroundColor Cyan
Write-Host "    Token: '$token' → '$Name'"
Write-Host "    Raiz:  $root"
Write-Host ""

# --------------------------------------------------------------------------
# Helpers
# --------------------------------------------------------------------------
function IsExcluded([string]$path) {
    foreach ($seg in ($path.Substring($root.Length) -split '[/\\]')) {
        if ($excludeDirs -contains $seg) { return $true }
    }
    return $false
}

function ShouldProcessContent([string]$filePath) {
    $ext = [System.IO.Path]::GetExtension($filePath)
    return $extensions -contains $ext
}

# --------------------------------------------------------------------------
# PASSO 1 — Substituição de conteúdo
# --------------------------------------------------------------------------
Write-Host "--- PASSO 1: substituição de conteúdo ---" -ForegroundColor Yellow

$contentEdits = 0

$allFiles = Get-ChildItem -Path $root -Recurse -File -ErrorAction SilentlyContinue |
    Where-Object { -not (IsExcluded $_.FullName) } |
    Where-Object { ShouldProcessContent $_.FullName } |
    Where-Object { $_.FullName -ne $scriptFile }

foreach ($file in $allFiles) {
    $text = [System.IO.File]::ReadAllText($file.FullName, [System.Text.Encoding]::UTF8)
    if ($text.Contains($token)) {
        $updated = $text.Replace($token, $Name)
        $occurrences = ([regex]::Matches($text, [regex]::Escape($token))).Count
        Write-Host "  EDIT ($occurrences ocorrências): $($file.FullName.Substring($root.Length + 1))"
        if ($Apply) {
            [System.IO.File]::WriteAllText($file.FullName, $updated, [System.Text.Encoding]::UTF8)
        }
        $contentEdits++
    }
}

Write-Host "  Total de arquivos com conteúdo a editar: $contentEdits"
Write-Host ""

# --------------------------------------------------------------------------
# PASSO 2 — Renomear arquivos e diretórios (mais profundos primeiro)
# --------------------------------------------------------------------------
Write-Host "--- PASSO 2: renomear arquivos e diretórios ---" -ForegroundColor Yellow

$renames = 0

# Coletar itens cujo nome contém o token (arquivos e dirs), mais profundos primeiro
$itemsToRename = Get-ChildItem -Path $root -Recurse -ErrorAction SilentlyContinue |
    Where-Object { -not (IsExcluded $_.FullName) } |
    Where-Object { $_.Name -like "*$token*" } |
    Sort-Object { $_.FullName.Length } -Descending  # mais profundos primeiro

foreach ($item in $itemsToRename) {
    $newName = $item.Name.Replace($token, $Name)
    $parentDir = if ($item -is [System.IO.FileInfo]) { $item.DirectoryName } else { $item.Parent.FullName }
    $newPath = Join-Path $parentDir $newName
    Write-Host "  RENAME: $($item.FullName.Substring($root.Length + 1)) → $newName"
    if ($Apply) {
        Rename-Item -Path $item.FullName -NewName $newName -Force
    }
    $renames++
}

Write-Host "  Total de itens a renomear: $renames"
Write-Host ""

# --------------------------------------------------------------------------
# Resumo
# --------------------------------------------------------------------------
Write-Host "=== Resumo ===" -ForegroundColor Cyan
Write-Host "  Arquivos com edição de conteúdo : $contentEdits"
Write-Host "  Arquivos/diretórios a renomear  : $renames"

if ($Apply) {
    Write-Host ""
    Write-Host "Rename aplicado com sucesso!" -ForegroundColor Green
    Write-Host "Abra a solução pelo novo arquivo .slnx gerado."
} else {
    Write-Host ""
    Write-Host "DRY-RUN concluído — nenhuma alteração foi feita." -ForegroundColor Yellow
    Write-Host "Execute com -Apply para efetivar: ./rename.ps1 -Name $Name -Apply"
}
