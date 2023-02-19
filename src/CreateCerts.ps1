dotnet dev-certs https -ep persist/certs/aspnet.pfx -p password
dotnet dev-certs https --trust

docker-compose -f docker-compose-setup.yml build
docker-compose -f docker-compose-setup.yml up --remove-orphans
$certs = Get-ChildItem persist/certs -Filter '*.crt' -Recurse | % {$_.FullName}
foreach ($cert in $certs){
	Import-Certificate -FilePath $cert -CertStoreLocation Cert:\LocalMachine\Root
}