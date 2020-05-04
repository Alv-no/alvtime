FROM node:13.12-alpine AS build-stage

WORKDIR /usr/src/app

COPY package*.json ./
RUN npm install --only=production

COPY . .
CMD npx tsc


FROM node:13.12-alpine AS prod-stage
WORKDIR /app
COPY --from=build-stage /usr/src/app/node_modules ./node_modules
COPY --from=build-stage /usr/src/app/dist ./dist
EXPOSE 80
CMD ["node", "dist/app.js"]
