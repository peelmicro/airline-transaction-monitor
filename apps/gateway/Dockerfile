FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5000

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["apps/gateway/Gateway.csproj", "apps/gateway/"]
COPY ["apps/common/Common.csproj", "apps/common/"]
RUN dotnet restore "apps/gateway/Gateway.csproj"
COPY apps/gateway/ apps/gateway/
COPY apps/common/ apps/common/
WORKDIR /src/apps/gateway
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "Gateway.dll"]
