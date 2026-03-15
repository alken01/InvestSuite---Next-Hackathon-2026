FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY InvestSuite.slnx .
COPY src/InvestSuite.Api/InvestSuite.Api.csproj src/InvestSuite.Api/
COPY tests/InvestSuite.Tests/InvestSuite.Tests.csproj tests/InvestSuite.Tests/
RUN dotnet restore

COPY . .
RUN dotnet publish src/InvestSuite.Api/InvestSuite.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

RUN mkdir -p /data
VOLUME /data

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "InvestSuite.Api.dll"]
