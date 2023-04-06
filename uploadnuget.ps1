$version="1.0.0-preview-1"
$accessToken="oy2eekax2ascnnsvvlpa7x665a2jkkruxkshtzu3mhcp4e"
$paths=ls src | select Name
for($x=0;$x -lt $paths.length; $x=$x+1)
{
$fp=-join ("src\",$paths[$x].Name,"\bin\Release\",$paths[$x].Name,".",$version,".nupkg");
dotnet nuget push $fp -k $accessToken -s https://api.nuget.org/v3/index.json
}