# Инструмент билда
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Где работаем
WORKDIR /src

# Собираем все (и цспрой и папку. Но делаем отдельно)
COPY ["SilesiaRental.MVC/SilesiaRental.MVC.csproj","SilesiaRental.MVC/"]

# Запускаем рестор для зависимостей и т.п.
RUN dotnet restore "SilesiaRental.MVC/SilesiaRental.MVC.csproj"

# Обязательное копирование остального кода
COPY . .

# Сборка
# Выбираем папку
WORKDIR "/src/SilesiaRental.MVC"
# Билдим
RUN dotnet publish "SilesiaRental.MVC.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:ErrorOnDuplicatePublishOutputFiles=false
#                                              ^ config    ^output       ^no exe              ^ даст перезаписать appsettings

# Завершаем
# через что
FROM mcr.microsoft.com/dotnet/aspnet:8.0
# куда
WORKDIR /app
# Спросить у чата
EXPOSE 8080

# Берем только важное
COPY --from=build /app/publish .

# Что запускаем (команда и что именно)
ENTRYPOINT ["dotnet", "SilesiaRental.MVC.dll"]