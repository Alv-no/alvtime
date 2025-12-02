import { getLocalProjects, setLocalProjects } from "@/composables/useLocalStorage";
import taskService from "@/services/taskService";
import type { Project, Task } from "@/types/ProjectTypes";
import { defineStore } from "pinia";
import { computed, ref } from "vue";
import Fuse from "fuse.js";

export const useTaskStore = defineStore("task", () => {
	const projects = ref<Project[] | null>(null);
	const favoriteProjects = ref<Project[]>([]);
	const editingProjectOrder = ref<boolean>(false);
	const filterQuery = ref<string>("");

	const getTasks = async () => {
		try {
			const response = await taskService.getProjects();
			if (response.status === 200) {
				projects.value = removeLockedTasksAndEmptyProjects(response.data as Project[]);

				const updatedTasks = getLocalProjects(projects.value);
				if (!updatedTasks) {
					setLocalProjects(projects.value ?? []);
				} else {
					projects.value = updatedTasks;
				}

				setFavoriteProjects();
			} else {
				console.error("Failed to fetch tasks:", response.statusText);
				projects.value = [];
			}
		} catch (error) {
			console.error("Error fetching tasks:", error);
			projects.value = [];
		}
	};

	const removeLockedTasksAndEmptyProjects = (projectsList: Project[]) => {
		return projectsList
			.map((project) => {
				const filteredTasks = project.tasks.filter((task) => !task.locked);
				return { ...project, tasks: filteredTasks };
			})
			.filter((project) => project.tasks.length > 0);
	};

	const updateTasks = async (tasksToUpdate: Task[]) => {
		await taskService.updateTasks(tasksToUpdate);
	};

	const toggleProjectExpandable = (projectId: string) => {
		const project = projects.value?.find((p: Project) => `${p.name}-${p.customerName}` === projectId);
		if (project) {
			project.open = !project.open;
		}

		setLocalProjects(projects.value ?? []);
		setFavoriteProjects();
	};

	const setFavoriteProjects = () => {
		if (!projects.value) return [];

		favoriteProjects.value = projects.value
			.map((project: Project) => {
				const favoriteTasks = project.tasks.filter((task: Task) => task.favorite);
				return favoriteTasks.length
					? { ...project, tasks: favoriteTasks }
					: null;
			})
			.filter((project): project is Project => project !== null).sort((a, b) => {
				return (a.index ?? 0) - (b.index ?? 0);
			});
	};

	const saveFavoritesOrderToProjects = async () => {
		if (!projects.value) return;

		projects.value.forEach((project) => {
			const favoriteProject = favoriteProjects.value.find(fp => fp.id === project.id);
			if (favoriteProject) {
				project.index = favoriteProject?.index;
			}
		});
	};

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
					name: "customerName",
					getFn: (project: Project) => project.customerName
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
		editingProjectOrder,
		filterQuery,
		filteredProjects,
		favoriteProjects,
		getTasks,
		updateTasks,
		toggleProjectExpandable,
		saveFavoritesOrderToProjects,
	};
});
