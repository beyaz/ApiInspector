$ProjectDirectory = "D:\work\git\ApiInspector\"
$FinalDirectory   = "D:\work\git\ApiInspector\Api Inspector (.net method invoker)\"
$WebUiOutputDirectory = $ProjectDirectory + "ApiInspector.WebUI\bin\Debug\net6.0\"

New-Item -ItemType Directory -Force -Path $FinalDirectory

$temp1 = $FinalDirectory + "wwwroot"
New-Item -ItemType Directory -Force -Path $temp1

$temp1 = $FinalDirectory + "ApiInspector.NetFramework"
New-Item -ItemType Directory -Force -Path $temp1

$temp1 = $FinalDirectory + "ApiInspector.NetCore"
New-Item -ItemType Directory -Force -Path $temp1


$temp1 = $ProjectDirectory + "ApiInspector.WebUI\wwwroot\index.js"
$temp2 = $FinalDirectory   + "wwwroot\index.js"
Copy-Item -Path $temp1 -Destination $temp2 -Force

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



$temp1 = $ProjectDirectory + "ApiInspector.Bootstrapper\ApiInspectorLatestVersion.zip"
Compress-Archive -Path $FinalDirectory -DestinationPath $temp1 -Force

Remove-Item -Path $FinalDirectory -Force -Recurse