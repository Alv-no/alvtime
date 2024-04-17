FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build-env
RUN apk add nodejs
RUN apk add npm
WORKDIR /app
COPY . ./
RUN npm --prefix Web install
RUN dotnet publish "Client/Alvtime.Adminpanel.Client.csproj" -c Release -o output

FROM nginx:alpine
WORKDIR /user/share/nginx/html
COPY --from=build-env /app/output/wwwroot .
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80