$content = Get-Content MainWindow.xaml -Raw
$content = $content -replace 'RowHeight="50"', 'RowHeight="Auto"'
$content | Set-Content MainWindow.xaml -Encoding UTF8
