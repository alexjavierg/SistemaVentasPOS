$content = Get-Content MainWindow.xaml.cs -Raw
$pattern = '(?s)\}\)\.GeneratePdf\(sfd\.FileName\);'
$replacement = @"
                        }).GeneratePdf(sfd.FileName);
                        
                        try 
                        {
                            var psi = new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true };
                            System.Diagnostics.Process.Start(psi);
                        }
                        catch { }
"@
$content -replace $pattern, $replacement | Set-Content MainWindow.xaml.cs -Encoding UTF8
