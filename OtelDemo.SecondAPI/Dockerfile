FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["OtelDemo.SecondAPI/OtelDemo.SecondAPI.csproj", "OtelDemo.SecondAPI/"]
RUN dotnet restore "OtelDemo.SecondAPI/OtelDemo.SecondAPI.csproj"
COPY . .
WORKDIR "/src/OtelDemo.SecondAPI"
RUN dotnet build "OtelDemo.SecondAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OtelDemo.SecondAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OtelDemo.SecondAPI.dll"]
