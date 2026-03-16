# Stage 1 — build the React app
FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json* ./
RUN npm ci --silent

COPY . .
RUN npm run build

# Stage 2 — serve with Nginx
FROM nginx:alpine AS final

# Copy built assets
COPY --from=build /app/dist /usr/share/nginx/html

# Nginx config: SPA routing + reverse proxy to Gateway
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
