# sync-addon.ps1 — mirror this addon into a Godot project's addons/ folder.
#
# Godot test projects consume a *copy* of addons/ogsa_telemetry, which
# silently drifts behind this repo. Run this after every addon change:
#
#   ./sync-addon.ps1 -Project "C:\path\to\MyGodotProject"
#
# -Project accepts the project directory or its project.godot.
# Mirrors addon/ogsa_telemetry exactly (stale files in the copy are
# deleted); .godot/ caches are left alone — Godot reimports on launch.
#
# Cross-platform: runs under Windows PowerShell 5.1 or pwsh (Linux/macOS).
# Uses a pure-PowerShell mirror instead of robocopy, which is Windows-only.

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Project
)

$ErrorActionPreference = 'Stop'

function Sync-Mirror {
    # Mirrors $Source into $Destination: copies new/changed files, deletes
    # stale files in the destination that no longer exist in the source
    # (except names matching $ExcludeFilter, which are left untouched),
    # and prunes directories left empty by deletions.
    param(
        [Parameter(Mandatory = $true)][string]$Source,
        [Parameter(Mandatory = $true)][string]$Destination,
        [string[]]$ExcludeFilter = @()
    )

    New-Item -ItemType Directory -Force -Path $Destination | Out-Null
    $srcFull = (Resolve-Path $Source).Path
    $dstFull = (Resolve-Path $Destination).Path

    $relPaths = @()
    foreach ($f in Get-ChildItem -Path $srcFull -Recurse -File) {
        $rel = $f.FullName.Substring($srcFull.Length).TrimStart('\', '/')
        $relPaths += $rel
        $dstFile = Join-Path $dstFull $rel
        $dstDir = Split-Path $dstFile -Parent
        if (-not (Test-Path $dstDir)) { New-Item -ItemType Directory -Force -Path $dstDir | Out-Null }
        $copy = $true
        if (Test-Path $dstFile) {
            $d = Get-Item $dstFile
            if ($d.Length -eq $f.Length -and $d.LastWriteTimeUtc -ge $f.LastWriteTimeUtc) { $copy = $false }
        }
        if ($copy) { Copy-Item -Path $f.FullName -Destination $dstFile -Force }
    }

    foreach ($f in Get-ChildItem -Path $dstFull -Recurse -File) {
        $excluded = $false
        foreach ($pattern in $ExcludeFilter) {
            if ($f.Name -like $pattern) { $excluded = $true; break }
        }
        if ($excluded) { continue }
        $rel = $f.FullName.Substring($dstFull.Length).TrimStart('\', '/')
        if ($relPaths -notcontains $rel) { Remove-Item -Path $f.FullName -Force }
    }

    # prune directories left empty by deletions (deepest first)
    Get-ChildItem -Path $dstFull -Recurse -Directory |
        Sort-Object -Property FullName -Descending |
        Where-Object { (Get-ChildItem -Path $_.FullName -Force | Measure-Object).Count -eq 0 } |
        Remove-Item -Force
}

$repoDir = $PSScriptRoot
$src = Join-Path $repoDir 'addon/ogsa_telemetry'
if (-not (Test-Path (Join-Path $src 'plugin.cfg'))) {
    throw "not run from the repo root: $src missing"
}

if ($Project.EndsWith('project.godot')) { $Project = Split-Path $Project -Parent }
$Project = (Resolve-Path $Project).Path
if (-not (Test-Path (Join-Path $Project 'project.godot'))) {
    throw "no project.godot found in $Project"
}

$dst = Join-Path $Project 'addons/ogsa_telemetry'
# exclude *.uid: Godot generates .uid sidecars in the project copy (like
# Unity's .meta files) — preserve them so references stay stable and no
# reimport churn
Sync-Mirror -Source $src -Destination $dst -ExcludeFilter @('*.uid')
Write-Host "synced -> $dst"
Write-Host "note: .godot/ caches may reimport on next editor launch (normal)."
exit 0
