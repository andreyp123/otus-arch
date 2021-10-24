FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS publish
ARG VERSION=0.0.1

WORKDIR /src
COPY ["HealthTest/HealthTest.csproj", "HealthTest/"]
RUN dotnet restore "HealthTest/HealthTest.csproj"
COPY . .
WORKDIR "/src/HealthTest"
RUN dotnet publish "HealthTest.csproj" -c Release -o /app/publish \
    --runtime linux-musl-x64 \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:Version="${VERSION}"

FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./HealthTest"]