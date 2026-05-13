FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Frontend/AssetFlow.BlazorUI/AssetFlow.BlazorUI.csproj \
    -c Release -o /app/publish

# Debug : lister ce qui est dans publish
RUN ls -la /app/publish/wwwroot/ && ls -la /app/publish/wwwroot/_framework/ || echo "No _framework folder"

FROM nginx:alpine
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80