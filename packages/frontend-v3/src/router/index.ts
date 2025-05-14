import { createMemoryHistory, createRouter } from "vue-router";

import HomeView from "../views/HomeView.vue";

const routes = [
	{
		path: "/",
		name: "home",
		component: HomeView,
	}
];

const router = createRouter({
	history: createMemoryHistory(),
	routes,
});

export { router };