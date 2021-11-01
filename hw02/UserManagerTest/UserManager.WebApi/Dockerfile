FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS publish
ARG VERSION=0.0.1

WORKDIR /src
COPY ["UserManager.Common/UserManager.Common.csproj", "UserManager.Common/"]
COPY ["UserManager.Repository/UserManager.Repository.csproj", "UserManager.Repository/"]
COPY ["UserManager.WebApi/UserManager.WebApi.csproj", "UserManager.WebApi/"]
COPY ["UserManager.Repository.Migration/UserManager.Repository.Migration.csproj", "UserManager.Repository.Migration/"]
RUN dotnet restore "UserManager.WebApi/UserManager.WebApi.csproj"
RUN dotnet restore "UserManager.Repository.Migration/UserManager.Repository.Migration.csproj"

COPY . .

WORKDIR "/src/UserManager.WebApi"
RUN dotnet publish "UserManager.WebApi.csproj" -c Release -o /app/publish \
    --runtime linux-musl-x64 \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:Version="${VERSION}"

WORKDIR "/src/UserManager.Repository.Migration"
RUN dotnet publish "UserManager.Repository.Migration.csproj" -c Release -o /app/migration \
    --runtime linux-musl-x64 \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:Version="${VERSION}"

FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine AS final
WORKDIR /migration
COPY --from=publish /app/migration .

WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./UserManager.WebApi"]