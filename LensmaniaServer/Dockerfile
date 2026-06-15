FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY LensmaniaLibrary/LensmaniaLibrary.csproj LensmaniaLibrary/
COPY LensmaniaServer/LensmaniaServer.csproj   LensmaniaServer/
RUN dotnet restore LensmaniaServer/LensmaniaServer.csproj

COPY LensmaniaLibrary/ LensmaniaLibrary/
COPY LensmaniaServer/  LensmaniaServer/
RUN dotnet publish LensmaniaServer/LensmaniaServer.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# curl sert uniquement au HEALTHCHECK du conteneur (image runtime sans client HTTP).
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

RUN mkdir -p /app/wwwroot/uploads/photos

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LensmaniaServer.dll"]