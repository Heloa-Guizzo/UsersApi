FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

COPY . .

RUN dotnet restore src/microservices/UsersAPI/UsersAPI.csproj

RUN dotnet publish src/microservices/UsersAPI/UsersAPI.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /publish .

ENTRYPOINT ["dotnet", "UsersAPI.dll"]