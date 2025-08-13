import { getLocalProjects, setLocalProjects } from "@/composables/useLocalProject";
import taskService from "@/services/taskService";
import type { Project, Task } from "@/types/ProjectTypes";
import { defineStore } from "pinia";
import { computed, ref } from "vue";
import Fuse from "fuse.js";

export const useTaskStore = defineStore("task", () => {
	const projects = ref<Project[] | null>(null);
	const filterQuery = ref<string>("");

	const getTasks = async () => {
		try {
			console.time("Getting tasks");
			const response = await taskService.getTasks();
			console.timeEnd("Getting tasks");
			if (response.status === 200) {
				console.time("Mutating tasks");
				projects.value = mutateTasks(response.data);
				console.timeEnd("Mutating tasks");
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
		const projectsMap = new Map<string, Project>();
		const filteredTaskList = taskList.filter(task => !task.locked);

		for (const task of filteredTaskList) {
			const projectId = task.project.id;
			const taskId = task.id;

			if (!projectsMap.has(projectId)) {
				projectsMap.set(projectId, { ...task.project, tasks: [], open: false });
			}

			const currentProject = projectsMap.get(projectId);
			if (currentProject && !currentProject.tasks.some((t: Task) => t.id === taskId)) {
				// eslint-disable-next-line @typescript-eslint/no-unused-vars
				const { project, ...rest } = task;
				currentProject.tasks.push(rest);
			}
		}

		return Array.from(projectsMap.values());
	};

	const favoriteProjects = computed(() => {
		if (!projects.value) return [];

		return projects.value
			.map((project: Project) => {
				const favoriteTasks = project.tasks.filter((task: Task) => task.favorite);
				return favoriteTasks.length
					? { ...project, tasks: favoriteTasks }
					: null;
			})
			.filter((project): project is Project => project !== null);
	});

	const filteredProjects = computed(() => {
		if (!filterQuery.value.trim() || !projects.value) {
			return projects.value;
		}
		const query = filterQuery.value;
		const fuse = new Fuse(projects.value, {
			keys: [
				{
					name: "name",
					getFn: (project: Project) => project.name
				},
				{
					name: "tasks.name",
					getFn: (project: Project) => project.tasks.map((task: Task) => task.name)
				},
				{
					name: "customer.name",
					getFn: (project: Project) => project.customer.name
				}
			],
			includeScore: true,
			threshold: 0.1,
			ignoreLocation: true,
		});
		const results = fuse.search(query);
		return results.map(result => result.item);
	});

	return {
		projects,
		filterQuery,
		filteredProjects,
		favoriteProjects,
		getTasks,
		updateTasks,
		toggleProjectExpandable
	};
});
