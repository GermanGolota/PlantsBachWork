wsl -d docker-desktop sysctl -w vm.max_map_count=262144

docker-compose -f docker-compose-env.yml down
docker-compose -f docker-compose-env.yml up --remove-orphans -d

cd back/Aggregates/Plants.Initializer
dotnet run
cd ../../..