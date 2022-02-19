FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS publish
ARG VERSION=0.0.1

WORKDIR /src
COPY ["Common/Common.csproj", "Common/"]
COPY ["Common.Events/Common.Events.csproj", "Common.Events/"]
COPY ["BillingSvc.Dal/BillingSvc.Dal.csproj", "BillingSvc.Dal/"]
COPY ["BillingSvc.Api/BillingSvc.Api.csproj", "BillingSvc.Api/"]
RUN dotnet restore "BillingSvc.Api/BillingSvc.Api.csproj"

COPY . .

WORKDIR "/src/BillingSvc.Api"
RUN dotnet publish "BillingSvc.Api.csproj" -c Release -o /app/publish \
    --runtime linux-musl-x64 \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:Version="${VERSION}"

FROM mcr.microsoft.com/dotnet/runtime-deps:6.0-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./BillingSvc.Api"]