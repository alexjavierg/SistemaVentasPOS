ALTER TABLE DetallesVentas ADD COLUMN NombreModeloSnapshot VARCHAR(255) DEFAULT '';
ALTER TABLE DetallesVentas ADD COLUMN TallaSnapshot VARCHAR(255) DEFAULT '';
ALTER TABLE DetallesVentas MODIFY COLUMN ModeloId INT NULL;

ALTER TABLE MovimientosInventario ADD COLUMN NombreModeloSnapshot VARCHAR(255) DEFAULT '';
ALTER TABLE MovimientosInventario MODIFY COLUMN ModeloId INT NULL;

ALTER TABLE DetallesVentas DROP FOREIGN KEY FK_DetallesVentas_Modelos_ModeloId;
ALTER TABLE DetallesVentas ADD CONSTRAINT FK_DetallesVentas_Modelos_ModeloId FOREIGN KEY (ModeloId) REFERENCES Modelos(Id) ON DELETE SET NULL;

ALTER TABLE MovimientosInventario DROP FOREIGN KEY FK_MovimientosInventario_Modelos_ModeloId;
ALTER TABLE MovimientosInventario ADD CONSTRAINT FK_MovimientosInventario_Modelos_ModeloId FOREIGN KEY (ModeloId) REFERENCES Modelos(Id) ON DELETE SET NULL;

ALTER TABLE InventariosSucursal DROP FOREIGN KEY FK_InventariosSucursal_Modelos_ModeloId;
ALTER TABLE InventariosSucursal ADD CONSTRAINT FK_InventariosSucursal_Modelos_ModeloId FOREIGN KEY (ModeloId) REFERENCES Modelos(Id) ON DELETE CASCADE;
