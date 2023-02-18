wsl -d docker-desktop sysctl -w vm.max_map_count=262144

docker-compose -f docker-compose.yml down --remove-orphans
Remove-Item persist -Recurse -Confirm:$false
git restore persist

docker-compose -f docker-compose-setup.yml build
docker-compose -f docker-compose-setup.yml up --remove-orphans
$certs = Get-ChildItem persist/certs -Filter '*.crt' -Recurse | % {$_.FullName}
foreach ($cert in $certs){
	Import-Certificate -FilePath $cert -CertStoreLocation Cert:\LocalMachine\Root
}
cp ./persist/certs/ca/ca.crt ./back/ca.crt
cp ./persist/certs/es01/es01.crt ./back/es01.crt

docker-compose -f docker-compose.yml build
docker-compose up --remove-orphans -d

del ./back/ca.crt
del ./back/es01.crt