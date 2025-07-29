import { api } from "@/services/apiClient";

export default {
	getTasks: async () => api.get("/api/user/tasks"),
	updateTasks: async (tasks: any[]) =>  api.post("/api/user/tasks", tasks),
};