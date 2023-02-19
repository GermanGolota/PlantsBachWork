wsl -d docker-desktop sysctl -w vm.max_map_count=262144

cp "./persist/certs/ca/ca.crt" "./back/ca.crt"
cp "./persist/certs/es01/es01.crt" "./back/es01.crt"

docker-compose -f docker-compose.yml build
docker-compose -f docker-compose.yml down
docker-compose -f docker-compose.yml up --remove-orphans -d

del "./back/ca.crt"
del "./back/es01.crt"