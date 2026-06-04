# Sử dụng SDK để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file csproj và restore dependencies
COPY ["BE.csproj", "./"]
RUN dotnet restore "BE.csproj"

# Copy toàn bộ mã nguồn
COPY . .
WORKDIR "/src/"
RUN dotnet build "BE.csproj" -c Release -o /app/build

# Publish ứng dụng
FROM build AS publish
RUN dotnet publish "BE.csproj" -c Release -o /app/publish

# Sử dụng runtime nhẹ để chạy
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Expose cổng 5265 hoặc dùng cổng Render gán
EXPOSE 80
EXPOSE 5265

ENTRYPOINT ["dotnet", "BE.dll"]
