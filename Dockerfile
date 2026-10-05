# ---------- Étape 1 : compilation (image lourde avec le SDK .NET) ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# On copie d'abord le .csproj seul : tant qu'il ne change pas,
# Docker réutilise le cache du "restore" (téléchargement des paquets NuGet).
COPY TaskFlow.csproj .
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# ---------- Étape 2 : exécution (image légère, runtime ASP.NET seulement) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Dossier de la base SQLite, appartenant à l'utilisateur non-root "app" fourni par l'image.
RUN mkdir -p /app/data && chown $APP_UID /app/data

COPY --from=build /app/publish .

USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TaskFlow.dll"]
