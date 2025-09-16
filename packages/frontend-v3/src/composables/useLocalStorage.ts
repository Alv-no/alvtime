import type { Project } from "@/types/ProjectTypes";

const setLocalProjects = (value: Project[]): void => {
	const localProjects = value.map(project => ({
		id: `${project.name}-${project.customerName}`,
		open: project.open,
		index: project.index || 0,
	}));

	localStorage.setItem("projects", JSON.stringify(localProjects));
};

const getLocalProjects = (projects: Project[]): Project[] | null => {
	const key = "projects";
	const item = localStorage.getItem(key);
	if (item) {
		try {
			const localProjects = JSON.parse(item) as Project[];

			return projects.map(project => {
				const localProject = localProjects.find(lp => lp.id === `${project.name}-${project.customerName}`);

				return {
					...project,
					open: localProject?.open ?? false,
					index: localProject?.index
				};
			});
		} catch (error) {
			console.error(`Error parsing localStorage item with key "${key}":`, error);
			return null;
		}
	}
	return null;
};

const setLocalSalary = (value: number | null): void => {
	if (value === null) {
		localStorage.removeItem("salary");
		return;
	}
	
	localStorage.setItem("salary", value.toString());
};

const getLocalSalary = (): number | null => {
	const item = localStorage.getItem("salary");
	if (item) {
		return parseFloat(item);
	}
	return null;
};

export {
	setLocalProjects,
	getLocalProjects,
	setLocalSalary,
	getLocalSalary,
};
