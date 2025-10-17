import type { RouteLocationNormalized, Router } from "vue-router";
import { msalInstance, loginRequest } from "../authConfig";
import { type PublicClientApplication, type RedirectRequest } from "@azure/msal-browser";
import { useUserStore } from "../stores/userStore";

export function registerGuard(router: Router) {
	router.beforeEach(async (to: RouteLocationNormalized) => {
		if (to.meta.requiresAuth) {
			const request = {
				...loginRequest,
				redirectStartPage: to.fullPath
			};

			const shouldProceed = await isAuthenticated(msalInstance, request);
			return shouldProceed || "/failed";
		}
    
		return true;
	});
}

export async function isAuthenticated(instance: PublicClientApplication, loginRequest: RedirectRequest): Promise<boolean> {    
	// If your application uses redirects for interaction, handleRedirectPromise must be called and awaited on each page load before determining if a user is signed in or not  
	return instance.handleRedirectPromise().then(() => {
		const userStore = useUserStore();
		const accounts = instance.getAllAccounts();
		if (accounts.length > 0) {
			if(!userStore.user) {
				// User is signed in, but user data is not set. Set user data.
				userStore.setUser(accounts[0]);
			}
			return true;
		}

		// User is not signed in and attempting to access protected route. Sign them in.
		return instance.loginRedirect(loginRequest).then(() => {
			return true;
		}).catch((e) => {
			console.error(e);
			return false;
		});

	}).catch(() => {
		return false;
	});
}