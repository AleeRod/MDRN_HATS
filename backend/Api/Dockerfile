FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY MDRN_HATS.sln ./
COPY backend/Api/MDRN_HATS.Api.csproj backend/Api/
COPY backend/Application/MDRN_HATS.Application.csproj backend/Application/
COPY backend/Domain/MDRN_HATS.Domain.csproj backend/Domain/
COPY backend/Infrastructure/MDRN_HATS.Infrastructure.csproj backend/Infrastructure/
COPY backend/Tests/MDRN_HATS.Tests.csproj backend/Tests/
RUN dotnet restore MDRN_HATS.sln
COPY backend/ backend/
RUN dotnet publish backend/Api/MDRN_HATS.Api.csproj --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "MDRN_HATS.Api.dll"]
