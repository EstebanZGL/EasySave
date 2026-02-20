FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["EasySave.LogServer/EasySave.LogServer.csproj", "EasySave.LogServer/"]
RUN dotnet restore "EasySave.LogServer/EasySave.LogServer.csproj"
COPY . .
WORKDIR "/src/EasySave.LogServer"
RUN dotnet build "EasySave.LogServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EasySave.LogServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
VOLUME /app/Logs
ENTRYPOINT ["dotnet", "EasySave.LogServer.dll"]