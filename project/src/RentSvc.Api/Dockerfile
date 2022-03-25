FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS publish
ARG VERSION=0.0.1

WORKDIR /src
COPY ["Common/Common.csproj", "Common/"]
COPY ["Common.Events/Common.Events.csproj", "Common.Events/"]
COPY ["Common.Tracing/Common.Tracing.csproj", "Common.Tracing/"]
COPY ["RentSvc.Dal/RentSvc.Dal.csproj", "RentSvc.Dal/"]
COPY ["RentSvc.Api/RentSvc.Api.csproj", "RentSvc.Api/"]
RUN dotnet restore "RentSvc.Api/RentSvc.Api.csproj"

COPY . .

WORKDIR "/src/RentSvc.Api"
RUN dotnet publish "RentSvc.Api.csproj" -c Release -o /app/publish \
    --runtime linux-musl-x64 \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:Version="${VERSION}"

FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./RentSvc.Api"]