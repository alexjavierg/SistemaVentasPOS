$content = Get-Content MainWindow.xaml.cs -Raw
$pattern = '(?s)\}\)\.GeneratePdf\(sfd\.FileName\);\s*try\s*\{\s*var psi = new System\.Diagnostics\.ProcessStartInfo\(sfd\.FileName\)\s*\{\s*UseShellExecute = true\s*\};\s*System\.Diagnostics\.Process\.Start\(psi\);\s*\}\s*catch\s*\{\s*\}'
$replacement = @"
                        }).GeneratePdf(sfd.FileName);
                        
                        try 
                        {
                            var psi = new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true };
                            System.Diagnostics.Process.Start(psi);
                        }
                        catch 
                        {
                            MessageBox.Show(`"El PDF se guardó correctamente en: `" + sfd.FileName + `"\n\nSin embargo, no se pudo abrir automáticamente porque no hay un visor de PDF predeterminado configurado en Windows.`", `"Documento Guardado`", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
"@
$content -replace $pattern, $replacement | Set-Content MainWindow.xaml.cs -Encoding UTF8
