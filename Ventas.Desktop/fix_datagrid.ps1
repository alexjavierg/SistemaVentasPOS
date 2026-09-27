$content = Get-Content HistorialVentasWindow.xaml -Raw
$pattern = '(?s)<DataGridTemplateColumn Width="200">.*?</DataGridTemplateColumn>'
$replacement = @"
                <DataGridTemplateColumn Width="220">
                    <DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,0,10,0">
                                <Button Content="Reimprimir" Style="{StaticResource MaterialDesignOutlinedButton}" Height="30" Margin="0,0,10,0" Tag="{Binding}" Click="btnReimprimir_Click"/>
                                <Button x:Name="btnAnular" Content="Anular" Background="#D32F2F" BorderBrush="#D32F2F" Foreground="White" Height="30" Width="70" Tag="{Binding}" Click="btnAnular_Click"/>
                            </StackPanel>
                            <DataTemplate.Triggers>
                                <DataTrigger Binding="{Binding Estado}" Value="Anulada">
                                    <Setter TargetName="btnAnular" Property="Visibility" Value="Hidden" />
                                </DataTrigger>
                            </DataTemplate.Triggers>
                        </DataTemplate>
                    </DataGridTemplateColumn.CellTemplate>
                </DataGridTemplateColumn>
"@
$content -replace $pattern, $replacement | Set-Content HistorialVentasWindow.xaml -Encoding UTF8
