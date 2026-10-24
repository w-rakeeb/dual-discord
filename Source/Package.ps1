$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$homeDir=Split-Path $PSScriptRoot -Parent
$source=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'UpdateService.cs') -Raw
$version=[regex]::Match($source,'CurrentVersion="([0-9.]+)"').Groups[1].Value
foreach($binary in 'Dual Discord.exe','Dual Discord Updater.exe'){
 $actual=[Reflection.AssemblyName]::GetAssemblyName((Join-Path $homeDir $binary)).Version.ToString(3)
 if($actual -ne $version){throw "Compiled version of $binary does not match CurrentVersion. Rebuild before publishing."}
}
$files=@('Dual Discord.exe','Dual Discord.exe.config','Dual Discord Updater.exe','README.md','LICENSE','Assets/Dual Discord.ico','Assets/Dual Discord.png','Assets/Dual Discord.svg','Assets/Discord.svg','Assets/Discord.png','Assets/Logo-source.json','Assets/app.manifest')
$release=Join-Path $homeDir 'Release'
New-Item -ItemType Directory -Path $release -Force | Out-Null
$file=Join-Path $release 'Dual-Discord-Windows.zip'
if(Test-Path -LiteralPath $file){Move-Item -LiteralPath $file -Destination (Join-Path $release ('Previous-'+[Guid]::NewGuid().ToString('N')+'.zip'))}
$zip=[IO.Compression.ZipFile]::Open($file,[IO.Compression.ZipArchiveMode]::Create)
try{foreach($name in $files){$source=Join-Path $homeDir $name;if(-not (Test-Path -LiteralPath $source -PathType Leaf)){throw "Missing release file: $name"};[void][IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,$source,$name,[IO.Compression.CompressionLevel]::Optimal)}}finally{$zip.Dispose()}
$hash=(Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant()
[IO.File]::WriteAllText((Join-Path $release 'SHA256SUMS.txt'),$hash+'  Dual-Discord-Windows.zip'+[Environment]::NewLine,[Text.UTF8Encoding]::new($false))
Write-Output $file
