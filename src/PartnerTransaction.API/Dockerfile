FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["src/PartnerTransaction.API/PartnerTransaction.API.csproj", "src/PartnerTransaction.API/"]

RUN dotnet restore "src/PartnerTransaction.API/PartnerTransaction.API.csproj"

COPY . .

WORKDIR "/src/src/PartnerTransaction.API"

RUN dotnet publish "PartnerTransaction.API.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "PartnerTransaction.API.dll"]