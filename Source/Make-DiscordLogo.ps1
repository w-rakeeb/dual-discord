$ErrorActionPreference='Stop'
if($PSVersionTable.PSEdition -eq 'Core' -or [Threading.Thread]::CurrentThread.GetApartmentState() -ne 'STA'){
 & "$env:WINDIR\System32\WindowsPowerShell\v1.0\powershell.exe" -NoProfile -STA -ExecutionPolicy Bypass -File $PSCommandPath
 if($LASTEXITCODE -ne 0){throw 'Discord logo rendering failed'}
 return
}
Add-Type -AssemblyName WindowsBase,PresentationCore,PresentationFramework
$assets=Join-Path (Split-Path $PSScriptRoot -Parent) 'Assets'
[xml]$svg=Get-Content -LiteralPath (Join-Path $assets 'Discord.svg') -Raw
$node=$svg.SelectSingleNode("//*[local-name()='path']")
$geometry=[Windows.Media.Geometry]::Parse('F1 '+$node.GetAttribute('d'))
$bounds=$geometry.Bounds
$visual=New-Object Windows.Media.DrawingVisual
$context=$visual.RenderOpen()
$scale=[Math]::Min(960/$bounds.Width,960/$bounds.Height)
$group=New-Object Windows.Media.TransformGroup
$group.Children.Add([Windows.Media.TranslateTransform]::new(-$bounds.X,-$bounds.Y))
$group.Children.Add([Windows.Media.ScaleTransform]::new($scale,$scale))
$offsetX=(1024-$bounds.Width*$scale)/2
$offsetY=(1024-$bounds.Height*$scale)/2
$group.Children.Add([Windows.Media.TranslateTransform]::new($offsetX,$offsetY))
$context.PushTransform($group)
$context.DrawGeometry([Windows.Media.BrushConverter]::new().ConvertFromString('#5865F2'),$null,$geometry)
$context.Close()
$bitmap=New-Object Windows.Media.Imaging.RenderTargetBitmap(1024,1024,96,96,[Windows.Media.PixelFormats]::Pbgra32)
$bitmap.Render($visual)
$encoder=New-Object Windows.Media.Imaging.PngBitmapEncoder
$encoder.Frames.Add([Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
$stream=[IO.File]::Create((Join-Path $assets 'Discord.png'))
try{$encoder.Save($stream)}finally{$stream.Dispose()}
