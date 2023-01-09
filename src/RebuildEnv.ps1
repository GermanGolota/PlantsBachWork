docker-compose -f docker-compose-env.yml down --remove-orphans
Remove-Item persist -Recurse -Confirm:$false
git restore persist
docker-compose -f docker-compose-setup.yml up --remove-orphans
$certs = Get-ChildItem persist/certs -Filter '*.crt' -Recurse | % {$_.FullName}
foreach ($cert in $certs){
	Import-Certificate -FilePath $cert -CertStoreLocation Cert:\LocalMachine\Root
}

docker-compose -f docker-compose-env.yml up --remove-orphans -d

$rootPath = Join-Path $pwd 'back/Plants.Presentation/wwwroot'
$env:WebRoot__Path = $rootPath

cd back/Plants.Initializer
dotnet run
cd ../..