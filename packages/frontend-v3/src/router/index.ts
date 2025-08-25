import { createWebHistory, createRouter, type RouteRecordRaw } from "vue-router";
import { registerGuard } from "./Guard";

const routes: Array<RouteRecordRaw> = [
	{
		path: "/",
		name: "home",
		component: () => import("@/views/HomeView.vue"),
		meta: {
			requiresAuth: true,
		}
	},
	{
		path: "/aktiviteter",
		name: "tasks",
		component: () => import("@/views/TaskView.vue"),
		meta: {
			requiresAuth: true,
		}
	},
	{
		path: "/timebank",
		name: "timebank",
		component: () => import("@/views/TimeBankView.vue"),
		meta: {
			requiresAuth: true,
		}
	},
	{
		path: "/failed",
		name: "failed",
		component: () => import("@/views/HomeView.vue"),
	}
];

const router = createRouter({
	history: createWebHistory(),
	routes,
});

registerGuard(router);

export { router };
