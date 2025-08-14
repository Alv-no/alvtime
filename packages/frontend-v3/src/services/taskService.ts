import { api } from "@/services/apiClient";
import type { Task } from "@/types/ProjectTypes";

export default {
	getProjects: async () => api.get("/api/user/projects"),
	updateTasks: async (tasks: Task[]) =>  api.post("/api/user/tasks", tasks),
};