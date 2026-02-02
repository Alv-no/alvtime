import type { RouteLocationNormalized, Router } from "vue-router";
import { useAuthStore } from "@/stores/authStore";

export function registerGuard(router: Router) {
	router.beforeEach(async (to: RouteLocationNormalized) => {
		if (to.meta.requiresAuth) {
			const authStore = useAuthStore();

			if (authStore.isLoading) {
				await authStore.checkAuth();
			}

			if (!authStore.isAuthenticated) {
				sessionStorage.setItem("redirectAfterLogin", to.fullPath);
				// authStore.login();
				return false;
			}

			return true;
		}

		return true;
	});
}