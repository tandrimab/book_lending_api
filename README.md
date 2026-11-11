# BookLending API

A library lending backend app in .NET 10: ASP.NET Core Web API, EF Core, SQL, Redis cache, Azure Service Bus and JWT auth.

### Prerequisite

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

### 1. Start SQL Server

docker run -d --name booklending-sql \
  -e "ACCEPT_EULA=Y" \
  -e "MSSQL_SA_PASSWORD=<password>" \
  -p 1433:1433 \
  mcr.microsoft.com/mssql/server:2022-latest


### 2. Start Redis
docker run -d --name redis -p 6380:6379 redis:7


### 3. Start the Service Bus emulator
cp docker/servicebus/.env.example docker/servicebus/.env   # then set MSSQL_SA_PASSWORD
docker compose -f docker/servicebus/docker-compose.yml up -d

# check its up
curl http://localhost:5300/health


### 4. Configure secrets

The Redis and Service Bus connection strings are added in `appsettings.Development.json`. The DB connection string, JWT signing key and demo login go in user secrets:

cd src/BookLending.Api

dotnet user-secrets set "ConnectionStrings:BookLending" "Server=localhost,1433;Database=BookLending;User Id=sa;Password=<password>;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "<openssl_rand>"
dotnet user-secrets set "Auth:DemoUser:Username" "librarian"
dotnet user-secrets set "Auth:DemoUser:Password" "<password>"


### 5. Create the database

Applies the migrations, including the stored procedures:

dotnet ef database update \
  --project src/BookLending.Infrastructure \
  --startup-project src/BookLending.Api


### 6. Run the API

dotnet run --project src/BookLending.Api


Then open Swagger UI at http://localhost:5161/swagger to try it out


To start the containers again later, run `docker start booklending-sql redis` plus the compose `up -d` command.