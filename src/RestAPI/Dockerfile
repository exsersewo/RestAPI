FROM mcr.microsoft.com/dotnet/sdk:5.0.102-ca-patch-buster-slim AS build
WORKDIR /app

COPY ["src/RestAPI/RestAPI.csproj", "src/RestAPI/"]
RUN dotnet restore "src/RestAPI/RestAPI.csproj"
COPY . .

WORKDIR ./src/RestAPI

RUN dotnet build "RestAPI.csproj" -c Release -o /app/build
RUN dotnet publish "RestAPI.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "RestAPI.dll"]