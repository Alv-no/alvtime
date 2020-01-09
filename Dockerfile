FROM node:13.5-alpine

WORKDIR /usr/src/app

COPY package*.json ./
RUN npm install

COPY . .
CMD sh
