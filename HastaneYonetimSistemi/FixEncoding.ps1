$replacements = @{
    "ÅŸ" = "ş"
    "Åž" = "Ş"
    "Ä±" = "ı"
    "Ä°" = "İ"
    "Ã§" = "ç"
    "Ã‡" = "Ç"
    "Ã¶" = "ö"
    "Ã–" = "Ö"
    "Ã¼" = "ü"
    "Ãœ" = "Ü"
    "ÄŸ" = "ğ"
    "Äž" = "Ğ"
}

$files = Get-ChildItem -Path "d:\projeler\Proje\webProje\HastaneYonetimSistemi\HastaneYonetimSistemi" -Include *.cs, *.cshtml -Recurse

foreach ($file in $files) {
    # Read content as UTF-8
    $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
    
    $changed = $false
    foreach ($key in $replacements.Keys) {
        if ($content -match [regex]::Escape($key)) {
            $content = $content -replace [regex]::Escape($key), $replacements[$key]
            $changed = $true
        }
    }
    
    if ($changed) {
        Write-Host "Fixing encoding in $($file.FullName)"
        # Write back as UTF-8 with BOM (default for Set-Content with UTF8 encoding in PS 5.1)
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8
    }
}
