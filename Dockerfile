# Use the official .NET 10 SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy the project files and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the source code and publish the project
COPY . .
RUN dotnet publish -c Release -o out

# Use the official .NET 10 runtime image for the final container
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Copy the build output from the previous stage
COPY --from=build /app/out .

# Expose the port the app runs on
EXPOSE 8080

# Set the entry point to run the application
ENTRYPOINT ["dotnet", "OnlineStore.dll"]