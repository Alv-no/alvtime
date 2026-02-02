// src/services/authService.ts
import axios from "axios";
import config from "@/config.ts";

export interface UserInfo {
  isAuthenticated: boolean;
  name?: string;
  email?: string;
  claims?: string[];
}

const authApi = axios.create({
	baseURL: config.API_HOST + "/api/auth",
	withCredentials: true,
});

export const authService = {
	login(returnUrl?: string): void {
		const currentPath = returnUrl ?? window.location.pathname + window.location.search;
		const encodedReturnUrl = encodeURIComponent(currentPath);
		window.location.href = `${config.API_HOST}/api/auth/login?returnUrl=${encodedReturnUrl}`;
	},

	logout(): void {
		window.location.href = `${config.API_HOST}/api/auth/logout`;
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