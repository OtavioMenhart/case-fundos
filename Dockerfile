FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Directory.Packages.props", "Directory.Packages.props"]
COPY ["CaseItau.API/CaseItau.API.csproj", "CaseItau.API/"]
COPY ["CaseItau.Application/CaseItau.Application.csproj", "CaseItau.Application/"]
COPY ["CaseItau.Domain/CaseItau.Domain.csproj", "CaseItau.Domain/"]
COPY ["CaseItau.Infra/CaseItau.Infra.csproj", "CaseItau.Infra/"]
RUN dotnet restore "CaseItau.API/CaseItau.API.csproj"
COPY . .
WORKDIR "/src/CaseItau.API"
RUN dotnet build "CaseItau.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CaseItau.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CaseItau.API.dll"]
