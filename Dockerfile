FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Frontend/AssetFlow.BlazorUI/AssetFlow.BlazorUI.csproj \
    -c Release -o /app/publish

FROM nginx:alpine
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY --from=build /src/src/Frontend/AssetFlow.BlazorUI/bin/Release/net10.0/wwwroot/_framework /usr/share/nginx/html/_framework
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80