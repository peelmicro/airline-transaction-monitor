FROM node:24-alpine AS build
WORKDIR /app
COPY apps/web/package*.json ./
RUN npm install
COPY apps/web/ .
RUN npx ng build --configuration production

FROM nginx:alpine AS final
COPY --from=build /app/dist/web/browser /usr/share/nginx/html
COPY apps/web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 4200
