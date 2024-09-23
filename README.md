# Bot A Bordel V2

[![Issues][issues-shield]][issues-url]
[![Quality Gate Status][quality-badge]][quality-url]

- [Bot A Bordel V2](#bot-a-bordel-v2)
  - [About The Project](#about-the-project)
    - [Built With](#built-with)
  - [Getting Started](#getting-started)
    - [Prerequisites](#prerequisites)
    - [Installation](#installation)

<!-- ABOUT THE PROJECT -->

## About The Project

A Discord music with some specific features and tweaks for use on a personal server.

The project is an Hosted Service interacting with Lavalink to search and interact with media. IaC Bicep templates are included for Azure deployment.

### Built With

- [![.NET][.NET]][dotnet-url]
- [Discord.Net][discord-net-url]
- [Lavalink4NET][lavalink4net-url]

<!-- GETTING STARTED -->

## Getting Started

The Bot service host needs to establish a connection to a Lavalink server to start properly.

The lavalink server can be started either manually by executing the `.jar` java executable or using Docker compose to start both images (Bot and Lavalink).

This section assumes Docker is used to start the Bot and its dependency.

### Prerequisites

- Ensure .NET 8 Sdk is installed
- Ensure Docker is installed and running.
- Create a Discord App and generate a bot token at [Discord Developer](https://discord.com/developers/applications).
  - Connect Bot to a server
  - Permissions include: [Slash Commands, Send Messages, Connect, Speak]
- To test track search from Spotify, an Spotify Application must be created with a generated client secret [Spotify Developer](https://developer.spotify.com/dashboard).

> ⚠️ **Note**: Spotify source is a track search source, not a streaming source. Music streaming of Spotify tracks still requires Youtube as a source.

- To limit Youtube API blocks on bots, a "Proof of Origin Token" may be generated and passed into config and help limiting 403 errors. See [Lavalink Youtube Plugin](https://github.com/lavalink-devs/youtube-source?tab=readme-ov-file#using-a-potoken)

### Installation

1. Create a Discord App and generate a bot token at [Discord Developer](https://discord.com/developers/applications).
2. Clone the repo

   ```sh
   git clone https://github.com/hmathieu31/Discord.BotABordelV2.git
   ```

3. In `Lavalink`, copy `.env.template` into `.env` and fill with your configuration.
4. Pull and Run Lavalink image

   ```sh
   docker pull ghcr.io/lavalink-devs/lavalink:4
   ```

   At repository root, run
  
   ```sh
    docker run -d \
    --name lavalink \
    --restart unless-stopped \
    -e _JAVA_OPTIONS="-Xmx6G" \
    --env-file ./Lavalink/.env \
    -v $(pwd)/Lavalink/application.yml:/opt/Lavalink/application.yml \
    -p 2333:2333 \
    ghcr.io/lavalink-devs/lavalink:<lavalink-version>
   ```

5. Enter your generated token in User Secrets (In Visual Studio, VS Code with [extension](https://marketplace.visualstudio.com/items?itemName=adrianwilczynski.user-secrets), or through environment variables) and add lavalink config

   ```json
   {
    "DiscordBot:Token": "your bot token",
    "Lavalink:Password": "youshallnotpass"
   }
   ```

6. Build Bot

   ```sh
   dotnet build
   ```

<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->

[issues-shield]: https://img.shields.io/github/issues/hmathieu31/Discord.BotABordelV2.svg
[issues-url]: https://github.com/hmathieu31/Discord.BotABordelV2/issues
[.NET]: https://img.shields.io/badge/.NET-5C2D91?style=for-the-badge&logo=.net&logoColor=white
[dotnet-url]: https://dotnet.microsoft.com/en-us/
[discord-net-url]: https://discordnet.dev/
[lavalink4net-url]: https://github.com/angelobreuer/Lavalink4NET
[quality-badge]: https://sonarcloud.io/api/project_badges/measure?project=hmathieu-insat_Discord.BotABordelV2&metric=alert_status
[quality-url]: https://sonarcloud.io/summary/new_code?id=hmathieu-insat_Discord.BotABordelV2
