FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["FreelancePlatformApi.csproj", "./"]
RUN dotnet restore "FreelancePlatformApi.csproj"

COPY . .
RUN dotnet publish "FreelancePlatformApi.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:$PORT
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE $PORT

ENTRYPOINT ["dotnet", "FreelancePlatformApi.dll"]
