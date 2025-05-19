import { createWebHistory, createRouter, type RouteRecordRaw } from "vue-router";
import { registerGuard } from "./Guard";

const routes: Array<RouteRecordRaw> = [
	{
		path: "/",
		name: "home",
		component: () => import("../views/HomeView.vue"),
		meta: {
			requiresAuth: true,
		}
	},
	{
		path: "/secret",
		name: "secret",
		component: () => import("../views/SecretView.vue"),
		meta: {
			requiresAuth: true,
		}
	},
	{
		path: "/failed",
		name: "failed",
		component: () => import("../views/HomeView.vue"),
	}
];

const router = createRouter({
	history: createWebHistory(),
	routes,
});

registerGuard(router);

export { router };
