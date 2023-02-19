wsl -d docker-desktop sysctl -w vm.max_map_count=262144

docker-compose -f docker-compose-env.yml down
docker-compose -f docker-compose-env.yml up --remove-orphans -d

$rootPath = Join-Path $pwd 'back/Plants.Presentation/wwwroot'
$env:WebRoot__Path = $rootPath

cd back/Aggregates/Plants.Initializer
dotnet run
cd ../../..