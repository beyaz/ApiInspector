$ProjectDirectory = "c:\github\ApiInspector\"
$FinalDirectory = [environment]::getfolderpath("mydocuments") + "\ApiInspector\Application\Api Inspector (.net method invoker)\";
$WebUiOutputDirectory = $ProjectDirectory + "ApiInspector.WebUI\bin\Debug\net6.0\"

New-Item -ItemType Directory -Force -Path $FinalDirectory

$temp1 = $FinalDirectory + "wwwroot"
New-Item -ItemType Directory -Force -Path $temp1

$temp1 = $FinalDirectory + "ApiInspector.NetFramework"
New-Item -ItemType Directory -Force -Path $temp1

$temp1 = $FinalDirectory + "ApiInspector.NetCore"
New-Item -ItemType Directory -Force -Path $temp1


$temp1 = $ProjectDirectory + "ApiInspector.WebUI\wwwroot\dist"
$temp2 = $FinalDirectory   + "wwwroot\"
Copy-Item -Path $temp1 -Destination $temp2 -Force -Recurse

$temp1 = $ProjectDirectory + "ApiInspector.WebUI\wwwroot\favicon.ico"
$temp2 = $FinalDirectory   + "wwwroot\favicon.ico"
Copy-Item -Path $temp1 -Destination $temp2 -Force


$temp1 = $WebUiOutputDirectory + "*.dll"
Copy-Item -Path $temp1 -Destination $FinalDirectory -Force

$temp1 = $WebUiOutputDirectory + "*.exe"
Copy-Item -Path $temp1 -Destination $FinalDirectory -Force

$temp1 = $WebUiOutputDirectory + "ApiInspector.WebUI.Config.json"
Copy-Item -Path $temp1 -Destination $FinalDirectory -Force

$temp1 = $WebUiOutputDirectory + "web.config"
Copy-Item -Path $temp1 -Destination $FinalDirectory -Force

$temp1 = $WebUiOutputDirectory + "ApiInspector.WebUI.runtimeconfig.json"
Copy-Item -Path $temp1 -Destination $FinalDirectory -Force




$temp1 = $ProjectDirectory + "ApiInspector.NetFramework\bin\Debug\*.exe"
$temp2 = $FinalDirectory   + "ApiInspector.NetFramework\"
Copy-Item -Path $temp1 -Destination $temp2 -Force

$temp1 = $ProjectDirectory + "ApiInspector.NetFramework\bin\Debug\*.dll"
$temp2 = $FinalDirectory   + "ApiInspector.NetFramework\"
Copy-Item -Path $temp1 -Destination $temp2 -Force



$temp1 = $ProjectDirectory + "ApiInspector.NetCore\bin\Debug\net6.0\*.dll"
$temp2 = $FinalDirectory   + "ApiInspector.NetCore\"
Copy-Item -Path $temp1 -Destination $temp2 -Force

$temp1 = $ProjectDirectory + "ApiInspector.NetCore\bin\Debug\net6.0\*.exe"
$temp2 = $FinalDirectory   + "ApiInspector.NetCore\"
Copy-Item -Path $temp1 -Destination $temp2 -Force

$temp1 = $ProjectDirectory + "ApiInspector.NetCore\bin\Debug\net6.0\ApiInspector.runtimeconfig.json"
$temp2 = $FinalDirectory   + "ApiInspector.NetCore\"
Copy-Item -Path $temp1 -Destination $temp2 -Force

### C O P Y   V E R S I O N 
$temp1 = $ProjectDirectory + "ApiInspector.Bootstrapper\Version.txt"
$temp2 = $FinalDirectory
Copy-Item -Path $temp1 -Destination $temp2 -Force

$exePath = $FinalDirectory + "ApiInspector.WebUI.exe"
& $exePath


$temp1 = $ProjectDirectory + "ApiInspector.Bootstrapper\ApiInspectorLatestVersion.zip"
Compress-Archive -Path $FinalDirectory -DestinationPath $temp1 -Force


