FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS publish
ARG VERSION=0.0.1

WORKDIR /src
COPY ["Common/Common.csproj", "Common/"]
COPY ["CarSvc.Dal/CarSvc.Dal.csproj", "CarSvc.Dal/"]
COPY ["CarSvc.Api/CarSvc.Api.csproj", "CarSvc.Api/"]
RUN dotnet restore "CarSvc.Api/CarSvc.Api.csproj"

COPY . .

WORKDIR "/src/CarSvc.Api"
RUN dotnet publish "CarSvc.Api.csproj" -c Release -o /app/publish \
    --runtime linux-musl-x64 \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:Version="${VERSION}"

FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./CarSvc.Api"]