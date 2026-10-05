# ---------- Étape 1 : compilation (image lourde avec le SDK .NET) ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# On copie d'abord le .csproj seul : tant qu'il ne change pas,
# Docker réutilise le cache du "restore" (téléchargement des paquets NuGet).
# ReadyToRun : code précompilé pour Linux x64 (instance t3.micro), démarrage plus rapide du conteneur.
COPY HxhGuide.csproj .
RUN dotnet restore -r linux-x64 -p:PublishReadyToRun=true

COPY . .
RUN dotnet publish -c Release -o /app/publish -r linux-x64 --self-contained false \
    -p:PublishReadyToRun=true --no-restore

# ---------- Étape 2 : exécution (image légère, runtime ASP.NET seulement) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Utilisateur non-root fourni par l'image officielle.
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "HxhGuide.dll"]
