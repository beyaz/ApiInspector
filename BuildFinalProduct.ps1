$ProjectDirectory = "D:\work\git\ApiInspector\"
$FinalDirectory   = "D:\work\git\ApiInspector\FinalProduct\"
$PublishDirectory = $ProjectDirectory + "ApiInspector.WebUI\bin\Release\netcoreapp3.1\publish\"


New-Item -ItemType Directory -Force -Path $FinalDirectory

$temp1 = $FinalDirectory + "wwwroot"
New-Item -ItemType Directory -Force -Path $temp1

$temp1 = $FinalDirectory + "ApiInspector.NetFramework"
New-Item -ItemType Directory -Force -Path $temp1

$temp1 = $FinalDirectory + "ApiInspector.NetCore"
New-Item -ItemType Directory -Force -Path $temp1


$temp1 = $PublishDirectory + "wwwroot\index.js"
$temp2 = $FinalDirectory   + "wwwroot\index.js"
Copy-Item -Path $temp1 -Destination $temp2 -Force



$temp1 = $PublishDirectory + "*.dll"
Copy-Item -Path $temp1 -Destination $FinalDirectory -Force

$temp1 = $PublishDirectory + "*.exe"
Copy-Item -Path $temp1 -Destination $FinalDirectory -Force

$temp1 = $PublishDirectory + "ApiInspector.WebUI.Config.json"
Copy-Item -Path $temp1 -Destination $FinalDirectory -Force

$temp1 = $PublishDirectory + "web.config"
Copy-Item -Path $temp1 -Destination $FinalDirectory -Force

$temp1 = $PublishDirectory + "ApiInspector.WebUI.runtimeconfig.json"
Copy-Item -Path $temp1 -Destination $FinalDirectory -Force




$temp1 = $ProjectDirectory + "ApiInspector.NetFramework\bin\Debug\*.exe"
$temp2 = $FinalDirectory   + "ApiInspector.NetFramework\"
Copy-Item -Path $temp1 -Destination $temp2 -Force

$temp1 = $ProjectDirectory + "ApiInspector.NetFramework\bin\Debug\*.dll"
$temp2 = $FinalDirectory   + "ApiInspector.NetFramework\"
Copy-Item -Path $temp1 -Destination $temp2 -Force



$temp1 = $ProjectDirectory + "ApiInspector.NetCore\bin\Debug\netcoreapp3.1\*.dll"
$temp2 = $FinalDirectory   + "ApiInspector.NetCore\"
Copy-Item -Path $temp1 -Destination $temp2 -Force

$temp1 = $ProjectDirectory + "ApiInspector.NetCore\bin\Debug\netcoreapp3.1\*.exe"
$temp2 = $FinalDirectory   + "ApiInspector.NetCore\"
Copy-Item -Path $temp1 -Destination $temp2 -Force


$temp1 = $ProjectDirectory + "ApiInspector.Bootstrapper\FinalProduct.zip"
Compress-Archive -Path $FinalDirectory -DestinationPath $temp1 -Force