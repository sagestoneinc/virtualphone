# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY VirtualPhone/VirtualPhone.csproj VirtualPhone/
RUN dotnet restore VirtualPhone/VirtualPhone.csproj
COPY VirtualPhone/ VirtualPhone/
RUN dotnet publish VirtualPhone/VirtualPhone.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "VirtualPhone.dll"]
