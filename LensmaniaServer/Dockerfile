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

# Le dossier des uploads doit appartenir a l'utilisateur applicatif (APP_UID)
# pour qu'il puisse y ecrire les photos une fois le process en non-root.
RUN mkdir -p /app/wwwroot/uploads/photos \
    && chown -R $APP_UID:$APP_UID /app/wwwroot/uploads

COPY --from=build /app/publish .

# On execute le process en non-root (utilisateur "app" fourni par l'image aspnet).
USER $APP_UID
ENTRYPOINT ["dotnet", "LensmaniaServer.dll"]