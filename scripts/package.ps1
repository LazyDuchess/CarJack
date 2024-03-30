param(
    [switch]$Release = $False
)

#Requires -Version 7.4
$ErrorActionPreference = 'Stop'
$PSNativeCommandUseErrorActionPreference = $true

Set-Location $PSScriptRoot/..
[Environment]::CurrentDirectory = (Get-Location -PSProvider FileSystem).ProviderPath

$csprojPath = "CarJack.Plugin/CarJack.Plugin.csproj"
$projxml = [xml](Get-Content -Path $csprojPath)
$version = $projxml.Project.PropertyGroup[0].Version

if($Release) {
    $Configuration='Release'
} else {
    $Configuration='Debug'
}

function EnsureDir($path) {
    if(!(Test-Path $path)) { New-Item -Type Directory $path > $null }
}

function Clean() {
    if(Test-Path Build) {
        Remove-Item -Recurse Build
    }
}

function CreateZip($zipPath) {
    if(Test-Path $zipPath) { Remove-Item $zipPath }
    $zip = [System.IO.Compression.ZipFile]::Open($zipPath, 'Create')
    return $zip
}

function ExtractZip($zipPath){
    $targetPath = [System.IO.Path]::Combine([System.IO.Path]::GetDirectoryName($zipPath),[System.IO.Path]::GetFileNameWithoutExtension($zipPath))
    [System.IO.Compression.ZipFile]::ExtractToDirectory($zipPath, $targetPath)
}

function AddToZip($zip, $path, $pathInZip=$path) {
    [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip, $path, $pathInZip) > $Null
}

function CreatePluginZip(){
    $zipPath = "Build/CarJack.Plugin.$Configuration-$version.zip"
    $readmePath = "README.md"
    $bundlePath = "CarJack.Editor/Build/carjack"
    $zip = CreateZip $zipPath

    Push-Location "CarJack.SlopCrew/bin~/$Configuration/net471"
    Get-ChildItem -Recurse './' -Exclude *.pdb,CommonAPI.dll,SlopCrew.API.dll | ForEach-Object {
        $path = ($_ | Resolve-Path -Relative).Replace('.\', '')
        AddToZip $zip $_.FullName $path
    }
    Pop-Location

    Push-Location "Thunderstore"
    Get-ChildItem -Recurse './' | ForEach-Object {
        $path = ($_ | Resolve-Path -Relative).Replace('.\', '')
        AddToZip $zip $_.FullName $path
    }
    Pop-Location


    if(Test-Path $readmePath){
        AddToZip $zip $readmePath $readmePath
    }

    if(Test-Path $bundlePath){
        AddToZip $zip $bundlePath "carjack"
    }

    $zip.Dispose()

    ExtractZip $zipPath
}

Clean
dotnet build -c $Configuration
EnsureDir "Build"
CreatePluginZip