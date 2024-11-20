FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
EXPOSE 5000

ENV DOTNET_USE_POLLING_FILE_WATCHER=true

CMD ["dotnet", "watch", "RUN", "--urls=http://0.0.0.0:5000"]

