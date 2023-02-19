## About
This is an application that is supposed to be used as a platform for plant-selling business.

It uses combined Elm-React frontend that calls to ASP.NET Core backend, which interacts with EventStore as the primary database and MongoDb+ElasticSearch as secondary storage systems.

You may find all of the documents that include techical tasks and all of the diagrams.

## Running the application
You can run the application in the docker-container that would spin up the database, add migrations to it and start backend and frontend.

To use it, you would need to trust self-signed SSl certificate, to do so run this script from src directory (you need to have an admin access to do so):
```bash
CreateCerts.ps1
```
If you have any issues with that you may refer to https://docs.microsoft.com/en-us/aspnet/core/security/docker-https?view=aspnetcore-6.0

To start the actual application you may then run the following from the src directory of the project: 
```powershell
StartProd.ps1
```
After that, the frontend application should be available at http://localhost:8002/login

If you wish to clear the application data, execute the folowing:
```powershell
DropPersistance.ps1
```