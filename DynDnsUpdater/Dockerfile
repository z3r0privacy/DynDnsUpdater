#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /src
COPY ["DynDnsUpdater/DynDnsUpdater.csproj", "DynDnsUpdater/"]
RUN dotnet restore "DynDnsUpdater/DynDnsUpdater.csproj"
COPY . .
WORKDIR "/src/DynDnsUpdater"
RUN dotnet build "DynDnsUpdater.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DynDnsUpdater.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DynDnsUpdater.dll"]

VOLUME /appdata