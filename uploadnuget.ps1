$version="1.0.0-preview-2"
$accessToken="oy2blj6dknoihbaote2alopztao2erfunet5yrfq7n3fqe"
$paths=ls src | select Name
for($x=0;$x -lt $paths.length+1; $x=$x+1)
{
$fp=-join ("src\",$paths[$x].Name,"\bin\Release\",$paths[$x].Name,".",$version,".nupkg");
dotnet nuget push $fp -k $accessToken -s https://api.nuget.org/v3/index.json
}