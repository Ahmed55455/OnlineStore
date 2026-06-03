# Use the build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy the project files and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the remaining files and build the project
COPY . ./
RUN dotnet publish -c Release -o out

# Use the final runtime environment
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose the port that Render will use
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Run the application (change the filename here if needed)
ENTRYPOINT ["dotnet", "OnlineStore.dll"]
