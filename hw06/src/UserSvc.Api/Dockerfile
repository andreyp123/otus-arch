FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS publish
ARG VERSION=0.0.1

WORKDIR /src
COPY ["Common/Common.csproj", "Common/"]
COPY ["UserSvc.Repository/UserSvc.Repository.csproj", "UserSvc.Repository/"]
COPY ["UserSvc.Api/UserSvc.Api.csproj", "UserSvc.Api/"]
RUN dotnet restore "UserSvc.Api/UserSvc.Api.csproj"

COPY . .

WORKDIR "/src/UserSvc.Api"
RUN dotnet publish "UserSvc.Api.csproj" -c Release -o /app/publish \
    --runtime linux-musl-x64 \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:Version="${VERSION}"

FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./UserSvc.Api"]