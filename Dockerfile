FROM node:12.16-alpine3.9 AS build-stage

WORKDIR /usr/src/app

COPY package*.json ./
RUN npm install

COPY . .
RUN npx tsc


FROM node:12.16-alpine3.9 AS prod-stage
WORKDIR /app
COPY --from=build-stage /usr/src/app/dist ./dist
COPY --from=build-stage /usr/src/app/package*.json ./
RUN npm install --only=production
EXPOSE 80
CMD ["node", "dist/app.js"]
