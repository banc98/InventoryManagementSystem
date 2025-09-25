# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore first (works when repo root is the project folder)
COPY ./InvApp.csproj ./
RUN dotnet restore ./InvApp.csproj

# Copy the rest and publish
COPY . .
RUN dotnet publish ./InvApp.csproj -c Release -o /app/out

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
# Render exposes PORT; bind Kestrel to it
ENV ASPNETCORE_URLS=http://0.0.0.0:${PORT}
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/out .
ENTRYPOINT ["dotnet","InvApp.dll"]
