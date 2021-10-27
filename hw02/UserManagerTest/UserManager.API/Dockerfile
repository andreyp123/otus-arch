FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS publish
ARG VERSION=0.0.1

WORKDIR /src
COPY ["UserManager.Common/UserManager.Common.csproj", "UserManager.Common/"]
COPY ["UserManager.DAL/UserManager.DAL.csproj", "UserManager.DAL/"]
COPY ["UserManager.API/UserManager.API.csproj", "UserManager.API/"]
RUN dotnet restore "UserManager.API/UserManager.API.csproj"

COPY . .
WORKDIR "/src/UserManager.API"
RUN dotnet publish "UserManager.API.csproj" -c Release -o /app/publish \
    --runtime linux-musl-x64 \
    --self-contained true \
    /p:PublishTrimmed=true \
    /p:Version="${VERSION}"

FROM mcr.microsoft.com/dotnet/runtime-deps:5.0-alpine AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./UserManager.API"]