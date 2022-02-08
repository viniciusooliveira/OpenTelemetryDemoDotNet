FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["OtelDemo.FirstAPI/OtelDemo.FirstAPI.csproj", "OtelDemo.FirstAPI/"]
RUN dotnet restore "OtelDemo.FirstAPI/OtelDemo.FirstAPI.csproj"
COPY . .
WORKDIR "/src/OtelDemo.FirstAPI"
RUN dotnet build "OtelDemo.FirstAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OtelDemo.FirstAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OtelDemo.FirstAPI.dll"]
