import type { TCompensationRate, TCustomer, THourRate, TProject, TTask } from '$lib/types';
import { get, writable } from 'svelte/store';
import { Customers, Projects, Tasks, CompensationRate, HourRate } from '$lib/mock/customers';
import { fetchProjects } from '$lib/service/projects';
import { fetchCustomers } from '$lib/service/customers';
import { fetchTasks } from '$lib/service/tasks';

type TCustomerStore = {
	customers: TCustomer[];
	projects: TProject[];
	tasks: TTask[];
	compensationRate: TCompensationRate[];
	hourRate: THourRate[];
	active: {
		customer: number | undefined;
		project: number | undefined;
		task: number | undefined;
	};
};

function createCustomers() {
	const customerStore = writable<TCustomerStore>({
		customers: Customers,
		projects: Projects,
		tasks: Tasks,
		compensationRate: CompensationRate,
		hourRate: HourRate,
		active: {
			customer: undefined,
			project: undefined,
			task: undefined
		}
	});
	const { subscribe, set, update } = customerStore;

	const loadCustomers = async (token: string) => {
		setCustomers(await fetchCustomers({ token }));
		setProjects(await fetchProjects({ token }));
		console.log('loaded');
	};

	const setCustomers = (customers: TCustomer[]) => {
		update((n) => {
			n.customers = customers;
			return n;
		});
	};

	const setProjects = (projects: TProject[]) => {
		update((n) => {
			n.projects = projects;
			return n;
		});
	};

	const setTasks = (tasks: TTask[]) => {
		update((n) => {
			n.tasks = tasks;
			return n;
		});
	};

	const setHourRates = (hourRate: THourRate[]) => {
		update((n) => {
			n.hourRate = hourRate;
			return n;
		});
	};

	const setCompensationRates = (compensationRate: TCompensationRate[]) => {
		update((n) => {
			n.compensationRate = compensationRate;
			return n;
		});
	};

	const setCustomer = async (Id: number, token: string) => {
		if (Id != get(customerStore).active.customer) {
			update((n) => {
				n.active.customer = Id;
				return n;
			});
			update((n) => {
				n.active.project = undefined;
				return n;
			});
			update((n) => {
				n.active.task = undefined;
				return n;
			});
			setTasks(await fetchTasks({ token: token, Ids: getProjects().map((p) => p.id) }));
		}
	};

	const setProject = (Id: number) => {
		if (Id != get(customerStore).active.project) {
			update((n) => {
				n.active.project = Id;
				return n;
			});
			update((n) => {
				n.active.task = undefined;
				return n;
			});
		}
	};

	const setTask = (Id: number) => {
		update((n) => {
			n.active.task = Id;
			return n;
		});
	};

	const getActiveCustomer = () => {
		const store = get(customerStore);
		return store.customers.find((c) => c.id == store.active.customer);
	};

	const getActiveProject = () => {
		const store = get(customerStore);
		return store.projects.find((p) => p.id == store.active.project);
	};

	const getActiveTask = () => {
		const store = get(customerStore);
		return store.tasks.find((t) => t.id == store.active.task);
	};

	const getProjects = () => {
		const store = get(customerStore);
		if (store.active.customer == undefined) {
			return [];
		}
		return store.projects.filter((p) => p.customer == store.active.customer);
	};

	const getTasks = () => {
		const store = get(customerStore);
		if (store.active.project == undefined) {
			return [];
		}
		return store.tasks.filter((t) => t.project == store.active.project);
	};

	const updateCustomer = (customer: TCustomer) => {
		const store = get(customerStore);
		const customers = store.customers;
		customers.map((c) => (c.id == store.active.customer ? customer : c));
		setCustomers(customers);
	};

	const updateProject = (project: TProject) => {
		const store = get(customerStore);
		const projects = store.projects;
		projects.map((p) => (p.id == store.active.project ? project : p));
		setProjects(projects);
	};

	/**
	 * Update an activity given a already active customer and active project.
	 *
	 * @param {TTask} Task - The new activity data to replace the existing activity with.
	 */
	const updateTask = (task: TTask, hourRate: THourRate, compensationRate: TCompensationRate) => {
		// Retrieve data from the customer store
		const store = get(customerStore);
		const tasks = store.tasks;
		const hourRates = store.hourRate;
		const activeTask = store.active.task;
		const compensationRates = store.compensationRate;
		tasks.map((t) => (t.id == activeTask ? task : t));
		hourRates.map((h) => (h.taskId == activeTask ? hourRate : h));
		compensationRates.map((c) => (c.taskId == activeTask ? compensationRate : c));
		setTasks(tasks);
		setHourRates(hourRates);
		setCompensationRates(compensationRates);
	};

	const addNewTask = (task: TTask) => {
		// Retrieve data from the customer store
		const store = get(customerStore);
		const tasks = [task, ...store.tasks];
		setTasks(tasks);
	};

	const addNewCustomer = (newCustomer: TCustomer) => {
		// Retrieve data from the customer store
		const store = get(customerStore);
		const customers = [newCustomer, ...store.customers];
		setCustomers(customers);
	};

	const addNewProject = (newProject: TProject) => {
		// Retrieve data from the customer store
		const store = get(customerStore);
		const projects = [newProject, ...store.projects];
		setProjects(projects);
	};

	return {
		subscribe,
		setCustomers,
		setProjects,
		setTasks,
		setCustomer,
		setProject,
		setTask,
		getActiveCustomer,
		getActiveProject,
		getActiveTask,
		loadCustomers,
		getProjects,
		getTasks,
		updateCustomer,
		updateProject,
		updateTask,
		addNewTask,
		addNewCustomer,
		addNewProject
	};
}

export const customers = createCustomers();
