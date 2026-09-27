$content = Get-Content Helpers/TicketPrinter.cs -Raw

$patternHeader = '(?s)headerRow\.Cells\.Add\(new TableCell\(new Paragraph\(new Run\(`"CANT`"\)\)\s*\{ FontWeight = FontWeights\.Bold, TextAlignment = TextAlignment\.Center, FontSize = 9 \}\)\);\s*headerRow\.Cells\.Add\(new TableCell\(new Paragraph\(new Run\(`"P\.UNIT`"\)\)\s*\{ FontWeight = FontWeights\.Bold, TextAlignment = TextAlignment\.Right, FontSize = 9 \}\)\);'
$replacementHeader = @"
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run(`"DOC`")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center, FontSize = 9 }));
            headerRow.Cells.Add(new TableCell(new Paragraph(new Run(`"P.DOCENA`")) { FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Right, FontSize = 9 }));
"@
$content = $content -replace $patternHeader, $replacementHeader

$patternLogoBorder1 = 'Border imageBorder = new Border \{ Width = 125, Height = 125, HorizontalAlignment = HorizontalAlignment.Center \};'
$replacementLogoBorder1 = 'Border imageBorder = new Border { Width = 150, HorizontalAlignment = HorizontalAlignment.Center };'
$content = $content -replace $patternLogoBorder1, $replacementLogoBorder1

$patternLogoBorder2 = 'Image imgLogo = new Image \{ Width = 125, Height = 125, Stretch = Stretch.Uniform, Source = bi \};'
$replacementLogoBorder2 = 'Image imgLogo = new Image { Width = 150, Stretch = Stretch.Uniform, Source = bi };'
$content = $content -replace $patternLogoBorder2, $replacementLogoBorder2

$content | Set-Content Helpers/TicketPrinter.cs -Encoding UTF8
