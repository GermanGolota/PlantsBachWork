docker-compose -f docker-compose-env.yml down --remove-orphans
Remove-Item persist
git restore persist
docker-compose -f docker-compose-setup.yml up --remove-orphans
$certs = Get-ChildItem persist/certs -Filter '*.crt' -Recurse | % {$_.FullName}
foreach ($cert in $certs){
	Import-Certificate -FilePath $cert -CertStoreLocation Cert:\LocalMachine\Root
}
docker-compose -f docker-compose-env.yml up --remove-orphans