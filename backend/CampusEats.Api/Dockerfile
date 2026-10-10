
# Stage 1: Build the ASP.NET Core API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY CampusEats.Api.csproj ./

RUN dotnet restore CampusEats.Api.csproj

COPY . .

RUN dotnet publish CampusEats.Api.csproj -c Release -o /app/publish --no-restore

# Stage 2: Run the application
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "CampusEats.Api.dll"]
