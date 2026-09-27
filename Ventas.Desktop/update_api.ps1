$content = Get-Content ../Ventas.Api/Controllers/VentasController.cs -Raw
$pattern = '(?s)// Descontar inventario\s*foreach \(var item in venta\.Detalles\).*?_context\.Ventas\.Add\(venta\);'
$replacement = @"
            // Validar y descontar inventario
            foreach (var item in venta.Detalles)
            {
                var inv = await _context.InventariosSucursal
                    .FirstOrDefaultAsync(i => i.ModeloId == item.ModeloId && i.SucursalId == venta.SucursalId);
                
                if (inv == null || inv.CantidadDocenas < item.CantidadDocenas)
                {
                    return BadRequest($`"No hay stock suficiente para el modelo con ID {item.ModeloId}. Stock actual: {inv?.CantidadDocenas ?? 0}`");
                }
                
                inv.CantidadDocenas -= item.CantidadDocenas;
            }

            _context.Ventas.Add(venta);
"@
$content -replace $pattern, $replacement | Set-Content ../Ventas.Api/Controllers/VentasController.cs -Encoding UTF8
