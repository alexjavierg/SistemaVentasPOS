$content = Get-Content MainWindow.xaml -Raw
$pattern = '(?s)<DataGrid x:Name="dgVentaActual".*?</DataGrid>'
$replacement = @"
            <DataGrid x:Name="dgVentaActual" Grid.Row="1" AutoGenerateColumns="False" CanUserAddRows="False" 
                      BorderThickness="0" Background="White" HeadersVisibility="Column" RowHeight="50" Margin="10,0">
                <DataGrid.Columns>
                    <DataGridTemplateColumn Header="Prod" Width="*">
                        <DataGridTemplateColumn.CellTemplate>
                            <DataTemplate>
                                <StackPanel VerticalAlignment="Center" Margin="5,2" >
                                    <TextBlock Text="{Binding Nombre}" FontWeight="SemiBold" TextWrapping="Wrap"/>
                                    <StackPanel Orientation="Horizontal" Margin="0,5,0,0">
                                        <Button Style="{DynamicResource MaterialDesignToolButton}" Height="25" Width="25" Click="btnRestarCarrito_Click" Tag="{Binding ModeloId}" Padding="0">
                                            <materialDesign:PackIcon Kind="Minus" Width="15" Height="15" Foreground="#D32F2F"/>
                                        </Button>
                                        <TextBox Text="{Binding CantidadDocenas, UpdateSourceTrigger=PropertyChanged}" 
                                                 FontSize="15" FontWeight="Black" Foreground="#1565C0" 
                                                 VerticalAlignment="Center" Margin="10,0" Width="40" 
                                                 TextAlignment="Center"
                                                 materialDesign:TextFieldAssist.DecorationVisibility="Hidden"
                                                 BorderThickness="0,0,0,1" BorderBrush="Gray"
                                                 PreviewTextInput="TxtCantidad_PreviewTextInput"
                                                 TextChanged="TxtCantidad_TextChanged"
                                                 LostFocus="TxtCantidad_LostFocus"
                                                 Tag="{Binding ModeloId}"/>
                                        <Button Style="{DynamicResource MaterialDesignToolButton}" Height="25" Width="25" Click="btnSumarCarrito_Click" Tag="{Binding ModeloId}" Padding="0">
                                            <materialDesign:PackIcon Kind="Plus" Width="15" Height="15" Foreground="#2E7D32"/>
                                        </Button>
                                    </StackPanel>
                                </StackPanel>
                            </DataTemplate>
                        </DataGridTemplateColumn.CellTemplate>
                    </DataGridTemplateColumn>
                    <DataGridTextColumn Header="Total" Binding="{Binding Total, StringFormat=N2}" Width="70">
                        <DataGridTextColumn.ElementStyle>
                            <Style TargetType="TextBlock">
                                <Setter Property="HorizontalAlignment" Value="Right"/>
                                <Setter Property="VerticalAlignment" Value="Center"/>
                            </Style>
                        </DataGridTextColumn.ElementStyle>
                    </DataGridTextColumn>
                    <DataGridTemplateColumn Width="70">
                        <DataGridTemplateColumn.CellTemplate>
                            <DataTemplate>
                                <Button Style="{StaticResource MaterialDesignIconButton}" ToolTip="Quitar" 
                                        Height="35" Width="35" Padding="0" Margin="0,0,10,0" Click="btnQuitarCarrito_Click" Tag="{Binding ModeloId}">
                                    <materialDesign:PackIcon Kind="TrashCan" Foreground="#D32F2F" Width="22" Height="22" HorizontalAlignment="Center" VerticalAlignment="Center"/>
                                </Button>
                            </DataTemplate>
                        </DataGridTemplateColumn.CellTemplate>
                    </DataGridTemplateColumn>
                </DataGrid.Columns>
            </DataGrid>
"@
$content -replace $pattern, $replacement | Set-Content MainWindow.xaml -Encoding UTF8
