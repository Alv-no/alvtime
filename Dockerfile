# build stage
FROM node:lts-alpine as build-stage
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .

ARG VUE_APP_HOST
ARG VUE_APP_ACCESS_SCOPE
ARG VUE_APP_CLIENT_ID
RUN npx vue-cli-service build

# production stage
FROM nginx:stable-alpine as production-stage
COPY --from=build-stage /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
