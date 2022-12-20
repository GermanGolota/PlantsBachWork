CALL docker-compose -f docker-compose-env.yml down --remove-orphans
CALL rmdir /s /q persist
CALL git restore persist
CALL docker-compose -f docker-compose-env.yml up --remove-orphans