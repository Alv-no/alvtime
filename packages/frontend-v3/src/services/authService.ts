import axios from "axios";
import config from "@/config.ts";

export interface UserInfo {
  isAuthenticated: boolean;
  name?: string;
}

const authApi = axios.create({
	baseURL: config.API_HOST + "/api/auth",
	withCredentials: true,
});

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
			const response = await authApi.get<UserInfo>("/userInfo");
			return response.data;
		} catch {
			return { isAuthenticated: false };
		}
	},
};