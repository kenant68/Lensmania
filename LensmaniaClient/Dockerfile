# ---------- Étape 1 : build WASM ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY LensmaniaLibrary/LensmaniaLibrary.csproj LensmaniaLibrary/
COPY LensmaniaClient/LensmaniaClient.csproj   LensmaniaClient/
RUN dotnet restore LensmaniaClient/LensmaniaClient.csproj

COPY LensmaniaLibrary/ LensmaniaLibrary/
COPY LensmaniaClient/  LensmaniaClient/
RUN dotnet publish LensmaniaClient/LensmaniaClient.csproj -c Release -o /app/publish

# ---------- Étape 2 : serveur web statique ----------
FROM nginx:1.27-alpine AS final
# Les fichiers publiés du WASM se trouvent dans publish/wwwroot.
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY LensmaniaClient/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80