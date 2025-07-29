import { getLocalProjects, setLocalProjects } from "@/composables/useLocalProject";
import taskService from "@/services/taskService";
import type { Project, Task } from "@/types/ProjectTypes";
import { defineStore } from "pinia";
import { computed, ref } from "vue";

export const useTaskStore = defineStore("task", () => {
	const tasks = ref<Project[] | null>(null);

	const getTasks = async () => {
		try {
			const response = await taskService.getTasks();
			if (response.status === 200) {
				tasks.value = mutateTasks(response.data);
				const updatedTasks = getLocalProjects(tasks.value);
				if (!updatedTasks) {
					setLocalProjects(tasks.value ?? []);
				} else {
					tasks.value = updatedTasks;
				}
			} else {
				console.error("Failed to fetch tasks:", response.statusText);
				tasks.value = [];
			}
		} catch (error) {
			console.error("Error fetching tasks:", error);
			tasks.value = [];
		}
	};

	const updateTasks = async (tasksToUpdate: Task[]) => {
		await taskService.updateTasks(tasksToUpdate);
		setLocalProjects(tasks.value ?? []);
	};

	const toggleProjectExpandable = (projectId: string) => {
		const project = tasks.value?.find((p: Project) => p.id === projectId);
		if (project) {
			project.open = !project.open;
		}
		setLocalProjects(tasks.value ?? []);
	};

	// eslint-disable-next-line @typescript-eslint/no-explicit-any
	const mutateTasks = (taskList: any[]) => {
		const projects: Project[] = [];

		for(const task of taskList) {
			const projectId = task.project.id;
			const taskId = task.id;

			if(!projects.some((project: { id: string; }) => project?.id === projectId)) {
				projects.push({...task.project, tasks: [], open: false });
			}

			const currentProject = projects.find(project => project.id === projectId);
			// eslint-disable-next-line @typescript-eslint/no-explicit-any
			if(!currentProject?.tasks.some((t: any) => t.id === taskId && t.project.id === projectId)) {
				// eslint-disable-next-line @typescript-eslint/no-unused-vars
				const { project, ...rest } = task;
				currentProject?.tasks.push(rest);
			}
		};

		return projects;
	};

	const favoriteTasks = computed(() => {
		const filteredProjects = tasks.value?.filter((project: Project) => {
			return project.tasks.some((task: Task) => task.favorite);
		}) || [];

		return filteredProjects.map((project: Project) => {
			return {
				...project,
				tasks: project.tasks.filter((task: Task) => task.favorite)
			};
		});
	});

	return { tasks, favoriteTasks, getTasks, updateTasks, toggleProjectExpandable };
});