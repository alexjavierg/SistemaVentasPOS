$replacements = @{
    'Ã“' = 'Ó'
    'Ã¡' = 'á'
    'Ã©' = 'é'
    'Ã³' = 'ó'
    'Ãº' = 'ú'
    'Ã±' = 'ñ'
    'Tǭctil' = 'Táctil'
    'Estǭndar' = 'Estándar'
    'Configuracin' = 'Configuración'
    'cdigo' = 'código'
    'Corazn' = 'Corazón'
    'ǟ?xito' = 'Éxito'
    'conexiǟn' = 'conexión'
    'vǟlido' = 'válido'
}

Get-ChildItem -Recurse -Include *.xaml,*.cs | ForEach-Object {
    $content = [System.IO.File]::ReadAllText($_.FullName)
    $modified = $false
    foreach ($key in $replacements.Keys) {
        if ($content.Contains($key)) {
            $content = $content.Replace($key, $replacements[$key])
            $modified = $true
        }
    }
    if ($modified) {
        [System.IO.File]::WriteAllText($_.FullName, $content, [System.Text.Encoding]::UTF8)
        Write-Host "Fixed $($_.Name)"
    }
}
