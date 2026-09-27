$content = Get-Content MainWindow.xaml -Raw
$content = $content -replace 'TICKET DE VENTA', 'NOTA DE VENTA'
$content | Set-Content MainWindow.xaml -Encoding UTF8
