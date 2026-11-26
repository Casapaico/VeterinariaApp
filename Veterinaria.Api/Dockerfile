# Se construye desde la raíz del repositorio (necesita las 4 capas):
#   docker build -f Veterinaria.Api/Dockerfile -t veterinaria-api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Veterinaria.Entidades/Veterinaria.Entidades.csproj Veterinaria.Entidades/
COPY Veterinaria.Datos/Veterinaria.Datos.csproj Veterinaria.Datos/
COPY Veterinaria.Negocio/Veterinaria.Negocio.csproj Veterinaria.Negocio/
COPY Veterinaria.Api/Veterinaria.Api.csproj Veterinaria.Api/
RUN dotnet restore Veterinaria.Api/Veterinaria.Api.csproj
COPY Veterinaria.Entidades/ Veterinaria.Entidades/
COPY Veterinaria.Datos/ Veterinaria.Datos/
COPY Veterinaria.Negocio/ Veterinaria.Negocio/
COPY Veterinaria.Api/ Veterinaria.Api/
RUN dotnet publish Veterinaria.Api/Veterinaria.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000
ENTRYPOINT ["dotnet", "Veterinaria.Api.dll"]
