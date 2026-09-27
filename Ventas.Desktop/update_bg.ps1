$content = Get-Content MainWindow.xaml -Raw
$pattern = 'Background="#E8F5E9"'
$replacement = 'Background="White"'
$content -replace $pattern, $replacement | Set-Content MainWindow.xaml -Encoding UTF8
