FROM mcr.microsoft.com/dotnet/core/aspnet:2.2-stretch-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2-stretch AS build
WORKDIR /src
COPY ["MailProxee.WebClient/MailProxee.WebClient.csproj", "MailProxee.WebClient/"]
COPY ["Module.EmailProxy/Module.EmailProxy.csproj", "Module.EmailProxy/"]
COPY ["Module.EmailProxy.Infrastructure.EntityFrameworkCore/Module.EmailProxy.Infrastructure.EntityFrameworkCore.csproj", "Module.EmailProxy.Infrastructure.EntityFrameworkCore/"]
RUN dotnet restore "MailProxee.WebClient/MailProxee.WebClient.csproj"
COPY . .
WORKDIR "/src/MailProxee.WebClient"
RUN dotnet build "MailProxee.WebClient.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "MailProxee.WebClient.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "MailProxee.WebClient.dll"]