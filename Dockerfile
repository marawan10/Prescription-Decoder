# Stage 1: Build React Client
FROM node:20 AS client-build
WORKDIR /app/client
COPY PrescriptionDecoder.Client/package*.json ./
RUN npm install
COPY PrescriptionDecoder.Client/ ./
RUN npm run build

# Stage 2: Build .NET API
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS api-build
WORKDIR /src
# Copy Solution and Project files first for better caching
COPY *.sln ./
COPY PrescriptionDecoder.API/*.csproj ./PrescriptionDecoder.API/
COPY PrescriptionDecoder.Domain/*.csproj ./PrescriptionDecoder.Domain/
COPY PrescriptionDecoder.Infrastructure/*.csproj ./PrescriptionDecoder.Infrastructure/
COPY PrescriptionDecoder.Application/*.csproj ./PrescriptionDecoder.Application/
# Restore dependencies
RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Copy built frontend assets to API's wwwroot
# Ensure the directory exists
RUN mkdir -p PrescriptionDecoder.API/wwwroot
COPY --from=client-build /app/client/dist ./PrescriptionDecoder.API/wwwroot

# Build and Publish API
WORKDIR /src/PrescriptionDecoder.API
RUN dotnet publish -c Release -o /app/publish

# Stage 3: Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=api-build /app/publish .

# Render exposes port 8080 by default
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "PrescriptionDecoder.API.dll"]
