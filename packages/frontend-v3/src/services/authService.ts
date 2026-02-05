import config from "@/config.ts";
import { api } from "./apiClient";

export interface UserInfo {
  isAuthenticated: boolean;
  name?: string;
}

export const authService = {
	login(returnUrl?: string): void {
		const currentPath = returnUrl ?? window.location.pathname + window.location.search;
		const fullReturnUrl = window.location.origin + currentPath;
		const encodedReturnUrl = encodeURIComponent(fullReturnUrl);
		window.location.href = `${config.API_HOST}/api/auth/login?returnUrl=${encodedReturnUrl}`;
	},

	logout(): void {
		const baseUrl = window.location.origin;
		window.location.href = `${config.API_HOST}/api/auth/logout?returnUrl=${baseUrl}`;
	},

	async getUserInfo(): Promise<UserInfo> {
		try {
			const response = await api.get<UserInfo>("/api/auth/userInfo");
			return response.data;
		} catch {
			return { isAuthenticated: false };
		}
	},
};