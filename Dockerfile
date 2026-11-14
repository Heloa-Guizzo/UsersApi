FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["UsersApi/UsersAPI.csproj", "UsersApi/"]
COPY ["Shared/Shared.csproj", "Shared/"]
RUN dotnet restore "UsersApi/UsersAPI.csproj"

COPY ["UsersApi/", "UsersApi/"]
COPY ["Shared/", "Shared/"]
WORKDIR "/src/UsersApi"
RUN dotnet publish "UsersAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "UsersAPI.dll"]