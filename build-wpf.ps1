# build-wpf.ps1

Write-Host "Instalando Obfuscar Global Tool..." -ForegroundColor Cyan
dotnet tool install --global Obfuscar.GlobalTool --ignore-failed-sources

Write-Host "1. Compilando el proyecto (Release)..." -ForegroundColor Cyan
dotnet build Ventas.Desktop\Ventas.Desktop.csproj -c Release

Write-Host "2. Ofuscando Ventas.Desktop.dll..." -ForegroundColor Cyan
obfuscar.console Ventas.Desktop\obfuscar.xml

if (!(Test-Path "Ventas.Desktop\bin\Release\net10.0-windows\win-x64\Obfuscated\Ventas.Desktop.dll")) {
    Write-Host "Error: La ofuscación falló. No se generó el archivo ofuscado." -ForegroundColor Red
    exit 1
}

Write-Host "3. Reemplazando DLL original con la ofuscada..." -ForegroundColor Cyan
Copy-Item "Ventas.Desktop\bin\Release\net10.0-windows\win-x64\Obfuscated\Ventas.Desktop.dll" "Ventas.Desktop\bin\Release\net10.0-windows\win-x64\Ventas.Desktop.dll" -Force

Write-Host "4. Empaquetando en un solo archivo (Single-File)..." -ForegroundColor Cyan
dotnet publish Ventas.Desktop\Ventas.Desktop.csproj -c Release --no-build -o ./Deploy_WPF/

Write-Host "¡Proceso completado! El archivo ejecutable está en Deploy_WPF/Ventas.Desktop.exe" -ForegroundColor Green

