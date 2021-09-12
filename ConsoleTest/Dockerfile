#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["ConsoleTest/ConsoleTest.csproj", "ConsoleTest/"]
RUN dotnet restore "ConsoleTest/ConsoleTest.csproj"
COPY . .
WORKDIR "/src/ConsoleTest"
RUN dotnet build "ConsoleTest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConsoleTest.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConsoleTest.dll"]