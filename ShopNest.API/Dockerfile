FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ShopNest.API/ShopNest.API.csproj", "ShopNest.API/"]
COPY ["ShopNest.Application/ShopNest.Application.csproj", "ShopNest.Application/"]
COPY ["ShopNest.Domain/ShopNest.Domain.csproj", "ShopNest.Domain/"]
COPY ["ShopNest.Infrastructure/ShopNest.Infrastructure.csproj", "ShopNest.Infrastructure/"]
RUN dotnet restore "ShopNest.API/ShopNest.API.csproj"
COPY . .
RUN dotnet publish "ShopNest.API/ShopNest.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ShopNest.API.dll"]
