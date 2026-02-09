import axios from "axios";
import config from "@/config";
import { useAuthStore } from "@/stores/authStore";

const api = axios.create({
	baseURL: config.API_HOST,
	withCredentials: true,
	headers: {
		"X-CSRF": "1"
	}
});

api.interceptors.response.use(
	(response) => response,
	(error) => {
		if (error.response?.status === 401) {
			const authStore = useAuthStore();
			authStore.login();
		}
		return Promise.reject(error);
	}
);

export { api };