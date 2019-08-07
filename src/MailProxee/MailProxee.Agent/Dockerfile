FROM mcr.microsoft.com/dotnet/core/runtime:2.2-stretch-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["MailProxee.Agent/MailProxee.Agent.csproj", "MailProxee.Agent/"]
COPY ["Module.EmailProxy/Module.EmailProxy.csproj", "Module.EmailProxy/"]
RUN dotnet restore "MailProxee.Agent/MailProxee.Agent.csproj"
COPY . .
WORKDIR "/src/MailProxee.Agent"
RUN dotnet build "MailProxee.Agent.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "MailProxee.Agent.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MailProxee.Agent.dll"]