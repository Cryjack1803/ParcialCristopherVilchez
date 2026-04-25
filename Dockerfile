FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["ParcialVilchezCristopher_.csproj", "./"]
RUN dotnet restore "ParcialVilchezCristopher_.csproj"

COPY . .
RUN dotnet publish "ParcialVilchezCristopher_.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "ParcialVilchezCristopher_.dll"]
