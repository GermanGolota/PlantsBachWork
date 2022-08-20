#Running the application
You can run the application in the docker-container that would spin up the database, add migrations to it and start backend and frontend
To use it, you would need to trust self-signed SSl certificate, to do so run:
```bash
dotnet dev-certs https -ep %USERPROFILE%\.aspnet\https\aspnetapp.pfx -p password
dotnet dev-certs https --trust
```
If you have any issues with that you may refer to https://docs.microsoft.com/en-us/aspnet/core/security/docker-https?view=aspnetcore-6.0

To start the actual application you may then run the following from the root of the project:
```bash
docker-compose -f src/docker-compose.yml up --remove-orphans
```
After that, the frontend application should be available at http://localhost:8002/login