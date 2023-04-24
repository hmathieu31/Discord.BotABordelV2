#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

RUN apt-get update && apt-get install libopus0 libopus-dev


FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Discord.BotABordelV2/Discord.BotABordelV2.csproj", "Discord.BotABordelV2/"]
RUN dotnet restore "Discord.BotABordelV2/Discord.BotABordelV2.csproj"
COPY . .
WORKDIR "/src/Discord.BotABordelV2"
RUN dotnet build "Discord.BotABordelV2.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Discord.BotABordelV2.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Discord.BotABordelV2.dll"]