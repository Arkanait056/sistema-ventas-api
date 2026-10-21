# Etapa 1: compilar con el SDK completo
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ApiDatos/ApiDatos.csproj ApiDatos/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore ApiDatos/ApiDatos.csproj
COPY ApiDatos/ ApiDatos/
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet publish ApiDatos/ApiDatos.csproj -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: imagen liviana solo para ejecutar
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "ApiDatos.dll"]