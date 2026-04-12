FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["apps/simulator/Simulator.csproj", "apps/simulator/"]
COPY ["apps/common/Common.csproj", "apps/common/"]
RUN dotnet restore "apps/simulator/Simulator.csproj"
COPY apps/simulator/ apps/simulator/
COPY apps/common/ apps/common/
WORKDIR /src/apps/simulator
RUN dotnet publish -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:5003
ENTRYPOINT ["dotnet", "Simulator.dll"]
