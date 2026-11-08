FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV DOTNET_GCHeapHardLimit=0x18000000
EXPOSE 10000
ENTRYPOINT ["dotnet", "wt-lab7-7postgres-efcore-valkovich.dll"]