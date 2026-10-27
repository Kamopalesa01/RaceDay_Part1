# Use the official ASP.NET Core runtime as a base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["RaceDay.Web/RaceDay.Web.csproj", "RaceDay.Web/"]
RUN dotnet restore "RaceDay.Web/RaceDay.Web.csproj"
COPY . .
WORKDIR "/src/RaceDay.Web"
RUN dotnet build "RaceDay.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RaceDay.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]