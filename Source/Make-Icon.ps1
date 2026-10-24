$ErrorActionPreference = 'Stop'
# Native vector rendering from the editable SVG composition.
if ($PSVersionTable.PSEdition -eq 'Core' -or [Threading.Thread]::CurrentThread.GetApartmentState() -ne 'STA') {
    & "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -STA -ExecutionPolicy Bypass -File $PSCommandPath
    if ($LASTEXITCODE -ne 0) { throw 'Icon rendering failed.' }
    return
}
Add-Type -AssemblyName WindowsBase,PresentationCore,PresentationFramework
$assetDir = Join-Path (Split-Path $PSScriptRoot -Parent) 'Assets'
[xml]$svg = Get-Content -LiteralPath (Join-Path $assetDir 'Dual Discord.svg') -Raw
$geometry = [Windows.Media.Geometry]::Parse('F1 '+$svg.svg.defs.path.d)
$geometry.Freeze()
function Brush($color) { return [Windows.Media.BrushConverter]::new().ConvertFromString($color) }
function Render-Logo([int]$size) {
    $visual = New-Object Windows.Media.DrawingVisual
    $context = $visual.RenderOpen()
    $context.PushTransform([Windows.Media.ScaleTransform]::new($size/256.0,$size/256.0))
    $tile = $svg.svg.rect
    $context.DrawRoundedRectangle((Brush $tile.fill),[Windows.Media.Pen]::new((Brush $tile.stroke),[double]$tile.'stroke-width'),[Windows.Rect]::new([double]$tile.x,[double]$tile.y,[double]$tile.width,[double]$tile.height),[double]$tile.rx,[double]$tile.rx)
    foreach($layer in $svg.svg.use) {
        $numbers = [regex]::Matches($layer.transform,'[-\d.]+') | ForEach-Object { [double]$_.Value }
        $context.PushTransform([Windows.Media.TranslateTransform]::new($numbers[0],$numbers[1]))
        $context.PushTransform([Windows.Media.ScaleTransform]::new($numbers[2],$numbers[2]))
        if ($layer.stroke) {
            $pen = [Windows.Media.Pen]::new((Brush $layer.stroke),[double]$layer.'stroke-width')
            $pen.LineJoin = [Windows.Media.PenLineJoin]::Round
            $context.DrawGeometry($null,$pen,$geometry)
        }
        $context.DrawGeometry((Brush $layer.fill),$null,$geometry)
        $context.Pop(); $context.Pop()
    }
    $context.Pop(); $context.Close()
    $bitmap = [Windows.Media.Imaging.RenderTargetBitmap]::new($size,$size,96,96,[Windows.Media.PixelFormats]::Pbgra32)
    $bitmap.Render($visual)
    $encoder = New-Object Windows.Media.Imaging.PngBitmapEncoder
    $encoder.Frames.Add([Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
    $stream = New-Object IO.MemoryStream
    $encoder.Save($stream)
    $bytes = $stream.ToArray(); $stream.Dispose()
    return ,$bytes
}
[IO.File]::WriteAllBytes((Join-Path $assetDir 'Dual Discord.png'),(Render-Logo 1024))
$sizes = @(16,20,24,32,40,48,64,128,256)
$images = @($sizes | ForEach-Object { ,(Render-Logo $_) })
$stream = [IO.File]::Create((Join-Path $assetDir 'Dual Discord.ico'))
$writer = New-Object IO.BinaryWriter $stream
try {
    $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$sizes.Count)
    $offset = 6 + 16*$sizes.Count
    for($i=0;$i -lt $sizes.Count;$i++) {
        $dimension = if($sizes[$i] -eq 256) { 0 } else { $sizes[$i] }
        $writer.Write([byte]$dimension); $writer.Write([byte]$dimension)
        $writer.Write([byte]0); $writer.Write([byte]0)
        $writer.Write([uint16]1); $writer.Write([uint16]32)
        $writer.Write([uint32]$images[$i].Length); $writer.Write([uint32]$offset)
        $offset += $images[$i].Length
    }
    foreach($bytes in $images) { $writer.Write([byte[]]$bytes) }
} finally { $writer.Dispose() }

# A distinct icon path refreshes existing shortcut caches without restarting Explorer.
Copy-Item -LiteralPath (Join-Path $assetDir 'Dual Discord.ico') -Destination (Join-Path $assetDir 'Dual Discord stacked.ico') -Force
