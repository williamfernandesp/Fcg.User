# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy csproj and restore first
COPY Fcg.User/src/Fcg.User.Web/Fcg.User.Web.csproj Fcg.User/src/Fcg.User.Web/
COPY Fcg.User/src/Fcg.User.Application/Fcg.User.Application.csproj Fcg.User/src/Fcg.User.Application/
COPY Fcg.User/src/Fcg.User.Infra/Fcg.User.Infra.csproj Fcg.User/src/Fcg.User.Infra/
COPY Fcg.User/src/Fcg.User.Proxy.Auth/Fcg.User.Proxy.Auth.csproj Fcg.User/src/Fcg.User.Proxy.Auth/
COPY Fcg.User/src/Fcg.User.Proxy.Games/Fcg.User.Proxy.Games.csproj Fcg.User/src/Fcg.User.Proxy.Games/
COPY Fcg.Payment/src/Fcg.Contracts/Fcg.Contracts.csproj Fcg.Payment/src/Fcg.Contracts/
RUN dotnet restore Fcg.User/src/Fcg.User.Web/Fcg.User.Web.csproj

# copy everything and publish
COPY Fcg.User Fcg.User
COPY Fcg.Payment Fcg.Payment
RUN dotnet publish Fcg.User/src/Fcg.User.Web/Fcg.User.Web.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Fcg.User.Web.dll"]
