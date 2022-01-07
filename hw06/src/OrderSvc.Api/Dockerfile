FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS publish
ARG VERSION=0.0.1

WORKDIR /src
COPY ["Common/Common.csproj", "Common/"]
COPY ["OrderSvc.Repository/OrderSvc.Repository.csproj", "OrderSvc.Repository/"]
COPY ["OrderSvc.Api/OrderSvc.Api.csproj", "OrderSvc.Api/"]
RUN dotnet restore "OrderSvc.Api/OrderSvc.Api.csproj"

COPY . .

WORKDIR "/src/OrderSvc.Api"
RUN dotnet publish "OrderSvc.Api.csproj" -c Release -o /app/publish \
    --runtime linux-musl-x64 \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:Version="${VERSION}"

FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./OrderSvc.Api"]