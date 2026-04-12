FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["apps/ingestion/Ingestion.csproj", "apps/ingestion/"]
COPY ["apps/common/Common.csproj", "apps/common/"]
RUN dotnet restore "apps/ingestion/Ingestion.csproj"
COPY apps/ingestion/ apps/ingestion/
COPY apps/common/ apps/common/
WORKDIR /src/apps/ingestion
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5001
ENTRYPOINT ["dotnet", "Ingestion.dll"]
