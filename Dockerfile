FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["FinTech.API/FinTech.API.csproj", "FinTech.API/"]
RUN dotnet restore "FinTech.API/FinTech.API.csproj"

COPY . .
WORKDIR "/src/FinTech.API"
RUN dotnet build "FinTech.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinTech.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet FinTech.API.dll"]
