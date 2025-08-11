import { getLocalProjects, setLocalProjects } from "@/composables/useLocalProject";
import taskService from "@/services/taskService";
import type { Project, Task } from "@/types/ProjectTypes";
import { defineStore } from "pinia";
import { computed, ref } from "vue";

export const useTaskStore = defineStore("task", () => {
	const projects = ref<Project[] | null>(null);

	const getTasks = async () => {
		try {
			const response = await taskService.getTasks();
			if (response.status === 200) {
				projects.value = mutateTasks(response.data);
				const updatedTasks = getLocalProjects(projects.value);
				if (!updatedTasks) {
					setLocalProjects(projects.value ?? []);
				} else {
					projects.value = updatedTasks;
				}
			} else {
				console.error("Failed to fetch tasks:", response.statusText);
				projects.value = [];
			}
		} catch (error) {
			console.error("Error fetching tasks:", error);
			projects.value = [];
		}
	};

	const updateTasks = async (tasksToUpdate: Task[]) => {
		await taskService.updateTasks(tasksToUpdate);
		setLocalProjects(projects.value ?? []);
	};

	const toggleProjectExpandable = (projectId: string) => {
		const project = projects.value?.find((p: Project) => p.id === projectId);
		if (project) {
			project.open = !project.open;
		}
		setLocalProjects(projects.value ?? []);
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

	const favoriteProjects = computed(() => {
		const filteredFavoriteProjects = filteredProjects.value?.filter((project: Project) => {
			return project.tasks.some((task: Task) => task.favorite);
		}) || [];

		return filteredFavoriteProjects.map((project: Project) => {
			return {
				...project,
				tasks: project.tasks.filter((task: Task) => task.favorite)
			};
		});
	});

	const filteredProjects = computed(() => {
		for(const project of projects.value || []) {
			project.tasks = project.tasks.filter((task: Task) => !task.locked);
		}
		return projects.value?.filter((project: Project) => project.tasks.length > 0) || [];
	});

	return { projects, favoriteProjects, filteredProjects, getTasks, updateTasks, toggleProjectExpandable };
});