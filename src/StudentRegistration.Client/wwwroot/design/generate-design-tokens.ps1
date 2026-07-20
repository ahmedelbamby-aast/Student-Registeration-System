param(
    [string] $InputPath = "$PSScriptRoot/design-tokens.json",
    [string] $OutputPath = "$PSScriptRoot/../css/design-tokens.css",
    [switch] $Verify
)

$document = Get-Content -Raw -LiteralPath $InputPath | ConvertFrom-Json -AsHashtable
$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add('/* Generated from design/design-tokens.json. Do not edit by hand. */')
$lines.Add(':root {')
$lines.Add('    color-scheme: light;')
foreach ($category in $document.tokens.Keys) {
    foreach ($token in $document.tokens[$category].Keys) {
        $value = $document.tokens[$category][$token]
        $lines.Add("    --srs-$category-$token`: $value`;")
    }
}
$lines.Add('}')
$lines.Add('')
$lines.Add('@media (prefers-reduced-motion: reduce) {')
$lines.Add('    :root { --srs-motion-duration-feedback: 0ms; --srs-motion-duration-overlay: 0ms; }')
$lines.Add('}')
$lines.Add('')
$lines.Add('@media (forced-colors: active) {')
$lines.Add('    :root { --srs-color-semantic-border: CanvasText; --srs-color-semantic-focus-ring: Highlight; --srs-focus-ring-color: Highlight; }')
$lines.Add('}')
$expected = ($lines -join "`n") + "`n"

if ($Verify) {
    $actual = (Get-Content -Raw -LiteralPath $OutputPath).Replace("`r`n", "`n")
    if ($actual -cne $expected) { throw 'design-tokens.css is not the exact deterministic projection of design-tokens.json.' }
    return
}

[System.IO.File]::WriteAllText((Resolve-Path (Split-Path $OutputPath)).Path + '/' + (Split-Path $OutputPath -Leaf), $expected, [System.Text.UTF8Encoding]::new($false))
