ARG NODE_VERSION=16
FROM node:${NODE_VERSION} AS dev

WORKDIR /src

COPY package*.json ./
RUN npm install

FROM dev AS build-stage

COPY . .
RUN npm run build-ts


FROM node:${NODE_VERSION}-alpine AS prod-stage
WORKDIR /app
COPY --from=build-stage /src/dist ./dist
COPY --from=build-stage /src/package*.json ./
COPY --from=build-stage /src/public ./public
RUN npm install --only=production
EXPOSE 80
CMD ["node", "dist/app.js"]
