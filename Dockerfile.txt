# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# copy csproj and restore first (better layer caching)
COPY ./src/InvApp/InvApp.csproj ./src/InvApp/
RUN dotnet restore ./src/InvApp/InvApp.csproj

# copy the rest and publish
COPY . .
RUN dotnet publish ./src/InvApp/InvApp.csproj -c Release -o /app/out

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
# Render provides PORT; bind Kestrel to it
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
# (Optional) ensure Production
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/out .
ENTRYPOINT ["dotnet","InvApp.dll"]
