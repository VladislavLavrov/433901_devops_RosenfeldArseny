FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5015

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["15_Calculator.csproj", "./"]
RUN dotnet restore "15_Calculator.csproj"
COPY . .
RUN dotnet publish "15_Calculator.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "15_Calculator.dll"]