$content = Get-Content Helpers/TicketPrinter.cs -Raw
$pattern = 'if \(!string\.IsNullOrEmpty\(configLocal\.LogoLocalName\) && File\.Exists\(configLocal\.LogoLocalName\)\)\s*\{\s*byte\[\] imageBytes = File\.ReadAllBytes\(configLocal\.LogoLocalName\);'
$replacement = @"
                string rutaLogo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, `"Images`", configLocal.LogoLocalName ?? `"`");
                if (!string.IsNullOrEmpty(configLocal.LogoLocalName) && File.Exists(rutaLogo))
                {
                    byte[] imageBytes = File.ReadAllBytes(rutaLogo);
"@
$content -replace $pattern, $replacement | Set-Content Helpers/TicketPrinter.cs -Encoding UTF8
