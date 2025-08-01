import type { Project } from "@/types/ProjectTypes";

const setLocalProjects = (value: Project[]): void => {
	const localProjects = value.map(project => ({
		id: project.id,
		tasks: project.tasks.map(task => ({
			id: task.id,
			enableComments: task.enableComments,
		})),
		open: project.open
	}));

	localStorage.setItem("projects", JSON.stringify(localProjects));
};

const getLocalProjects = (projects: Project[]): Project[] | null => {
	// TODO: Map localStorage items to Project type
	const key = "projects";
	const item = localStorage.getItem(key);
	if (item) {
		try {
			const localProjects = JSON.parse(item) as Project[];

			return projects.map(project => {
				const localProject = localProjects.find(lp => lp.id === project.id);

				return {
					...project,
					id: project?.id, // Ensure id is always present and non-undefined
					tasks: project.tasks.map(task => {
						const localTask = localProject?.tasks.find(lt => lt.id === task.id);

						return {
							...task,
							id: task.id, // Ensure id is always present and non-undefined
							enableComments: localTask?.enableComments ?? false
						};
					}),
					open: localProject?.open ?? false
				};
			});
		} catch (error) {
			console.error(`Error parsing localStorage item with key "${key}":`, error);
			return null;
		}
	}
	return null;
};

export {
	setLocalProjects,
	getLocalProjects,
};