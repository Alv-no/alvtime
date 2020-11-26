up:
	docker-compose up --detach

frontend: up
	docker-compose exec alvtime-vue-pwa npx vue-cli-service serve

adminpanel: up
	docker-compose exec alvtime-admin-react-pwa npx react-scripts start

down:
	docker-compose down --volumes

logs:
	docker-compose logs --follow

pull:
	docker-compose pull

build:
	docker-compose build

test:
	docker-compose exec alvtime-admin-react-pwa npx react-scripts test

prune:
	docker system prune --force
