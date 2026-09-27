$content = Get-Content Helpers/TicketPrinter.cs -Raw

$patternLogo = '(?s)if \(!string\.IsNullOrEmpty\(configLocal\.LogoLocalName\) && File\.Exists\(rutaLogo\)\)\s*\{\s*byte\[\] imageBytes = File\.ReadAllBytes\(rutaLogo\);'
$replacementLogo = @"
                bool tieneLogo = false;
                if (!string.IsNullOrEmpty(configLocal.LogoLocalName) && File.Exists(rutaLogo))
                {
                    tieneLogo = true;
                    byte[] imageBytes = File.ReadAllBytes(rutaLogo);
"@
$content = $content -replace $patternLogo, $replacementLogo

$patternHeader = '(?s)// 2\. CABECERA\s*Paragraph header = new Paragraph \{ TextAlignment = TextAlignment\.Center, Margin = new Thickness\(0\) \};\s*header\.Inlines\.Add\(new Run\(sucursal\.Nombre \+ `"\\n`"\) \{ FontWeight = FontWeights\.Black, FontSize = 14 \}\);'
$replacementHeader = @"
            // 2. CABECERA
            Paragraph header = new Paragraph { TextAlignment = TextAlignment.Center, Margin = new Thickness(0) };
            if (!tieneLogo)
            {
                header.Inlines.Add(new Run(sucursal.Nombre + `"\n`") { FontWeight = FontWeights.Black, FontSize = 14 });
            }
"@
$content = $content -replace $patternHeader, $replacementHeader

$content | Set-Content Helpers/TicketPrinter.cs -Encoding UTF8
