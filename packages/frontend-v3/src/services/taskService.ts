import { api } from "@/services/apiClient";

export default {
	getTasks: async () => api.get("/api/user/tasks"),
};