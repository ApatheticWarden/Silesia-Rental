#Image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Take all separately
COPY ["SilesiaRental/SilesiaRental.csproj", "SilesiaRental/"]
RUN dotnet restore "SilesiaRental/SilesiaRental.csproj"

# Copy all into container
COPY . .

#Build
WORKDIR "/src/SilesiaRental"
RUN dotnet publish "SilesiaRental.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080

# Take binaries
COPY --from=build /app/publish .

# What to run
ENTRYPOINT ["dotnet", "SilesiaRental.dll"]