FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS publish
ARG VERSION=0.0.1

WORKDIR /src
COPY ["Common/Common.csproj", "Common/"]
COPY ["NotificationSvc.Dal/NotificationSvc.Dal.csproj", "NotificationSvc.Dal/"]
COPY ["NotificationSvc.Api/NotificationSvc.Api.csproj", "NotificationSvc.Api/"]
RUN dotnet restore "NotificationSvc.Api/NotificationSvc.Api.csproj"

COPY . .

WORKDIR "/src/NotificationSvc.Api"
RUN dotnet publish "NotificationSvc.Api.csproj" -c Release -o /app/publish \
    --runtime linux-musl-x64 \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:Version="${VERSION}"

FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./NotificationSvc.Api"]