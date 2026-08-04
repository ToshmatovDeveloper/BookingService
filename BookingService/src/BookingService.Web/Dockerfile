FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /src

COPY ["BookingService.sln", "."]
COPY ["src/BookingService.Web/BookingService.Web.csproj", "src/BookingService.Web/"]
COPY ["src/BookingService.Domain/BookingService.Domain.csproj", "src/BookingService.Domain/"]
COPY ["src/BookingService.Infrastructure/BookingService.Infrastructure.csproj", "src/BookingService.Infrastructure/"]
COPY ["src/BookingService.Application/BookingService.Application.csproj", "src/BookingService.Application/"]
COPY ["src/BookingService.Presenters/BookingService.Presenters.csproj", "src/BookingService.Presenters/"]
COPY ["src/auth/BookingService.Auth/BookingService.Auth.csproj", "src/auth/BookingService.Auth/"]
COPY ["src/BookingService.Grpc.Contracts/BookingService.Grpc.Contracts.csproj", "src/BookingService.Grpc.Contracts/"]

RUN dotnet restore "src/BookingService.Web/BookingService.Web.csproj"

COPY . .
RUN dotnet build "src/BookingService.Web/BookingService.Web.csproj" -c Release

FROM builder AS publish
RUN dotnet publish "src/BookingService.Web/BookingService.Web.csproj" -c Release -o /app/publish --no-build

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 5000 5001
ENTRYPOINT ["dotnet", "BookingService.Web.dll"]