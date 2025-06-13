import taskService from "@/services/taskService";
import { defineStore } from "pinia";
import { ref } from "vue";

export const useTaskStore = defineStore("task", () => {
	const tasks = ref<any[] | null>(null);

	const getTasks = async () => {
		try {
			const response = await taskService.getTasks();
			if (response.status === 200) {
				tasks.value = mutateTasks(response.data);
			} else {
				console.error("Failed to fetch tasks:", response.statusText);
				tasks.value = [];
			}
		} catch (error) {
			console.error("Error fetching tasks:", error);
			tasks.value = [];
		}
	};

	const mutateTasks = (taskList: any[]) => {
		const projects: any[] = [];

		for(const task of taskList) {
			const projectId = task.project.id;
			const taskId = task.id;

			if(!projects.some((project: { id: any; }) => project?.id === projectId)) {
				projects.push({...task.project, tasks: [] });
			}

			const currentProject = projects.find(project => project.id === projectId);
			if(!currentProject.tasks.some((t: any) => t.id === taskId && t.project.id === projectId)) {
				// eslint-disable-next-line @typescript-eslint/no-unused-vars
				const { project, ...rest } = task;
				currentProject.tasks.push(rest);
			}
		};

		return projects;
	};

	return { tasks, getTasks };
});