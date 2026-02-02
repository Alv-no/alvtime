import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { authService, type UserInfo } from "@/services/authService";

export const useAuthStore = defineStore("auth", () => {
	const user = ref<UserInfo | null>(null);
	const isLoading = ref(true);

	const isAuthenticated = computed(() => user.value?.isAuthenticated ?? false);

	async function checkAuth(): Promise<void> {
		isLoading.value = true;
		try {
			user.value = await authService.getUserInfo();

			if (user.value.isAuthenticated) {
				const redirectPath = sessionStorage.getItem("redirectAfterLogin");
				if (redirectPath) {
					sessionStorage.removeItem("redirectAfterLogin");
					window.location.href = redirectPath;
				}
			}
		} catch {
			user.value = { isAuthenticated: false };
		} finally {
			isLoading.value = false;
		}
	}

	function login(): void {
		authService.login();
	}

	function logout(): void {
		authService.logout();
	}

	return {
		user,
		isAuthenticated,
		isLoading,
		checkAuth,
		login,
		logout,
	};
});