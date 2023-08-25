import type { TCompensationRate, TCustomer, THourRate, TProject, TTask } from "$lib/types";
import { get, writable } from "svelte/store";
import { Customers, Projects, Tasks, CompensationRate, HourRate}  from "$lib/mock/customers";

type TCustomerStore = {
	customers: TCustomer[],
	projects: TProject[],
	tasks: TTask[],
	compensationRate: TCompensationRate[],
	hourRate: THourRate[],
	active: {
		customer: number | undefined,
		project: number | undefined,
		task: number | undefined
	}
}

function createCustomers() {
	const customerStore = writable<TCustomerStore>(
		{
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
		}
	);
	const { subscribe, set, update } = customerStore;

	const fetchCustomers = () => {
		//fetch from backend
	}

	const setCustomers = (customers: TCustomer[]) => {
		update((n) => {n.customers = customers; return n})
	}

	const setProjects = (projects: TProject[]) => {
		update((n) => {n.projects = projects; return n})
	}

	const setTasks = (tasks: TTask[]) => {
		update((n) => {n.tasks = tasks; return n})
	}

    const setCustomer = (Id: number) => {
		if (Id != get(customerStore).active.customer) {
			update((n) => {n.active.customer = Id; return n})
			update((n) => {n.active.project = undefined; return n})
			update((n) => {n.active.task = undefined; return n})
		}
	}

	const setProject = (Id: number) => {
		if (Id != get(customerStore).active.project) {
			update((n) => {n.active.project = Id; return n})
			update((n) => {n.active.task = undefined; return n})
		} 
	}

	const setTask = (Id: number) => {
		update((n) => {n.active.task = Id; return n})
	}

	const getActiveCustomer = () => {
		const store = get(customerStore)
		return store.customers.find((c) => c.Id == store.active.customer)
	}

	const getActiveProject = () => {
		const store = get(customerStore)
		return store.projects.find((p) => p.Id == store.active.project)
	}

	const getActiveTask = () => {
		const store = get(customerStore)
		return store.tasks.find((t) => t.Id == store.active.task)
	}

	/* const getProjects = () => {
		const store = get(customerStore)
		return store.projects.filter((p) => p.CustomerId == store.active.customer)
	} */

	/* const getActivities = () => {
		const store = get(customerStore)
		return store.tasks.filter((t) => t.ProjectId == store.active.project)
	} */

	const updateCustomer = (customer: TCustomer) => {
		const store = get(customerStore)
		const customers = store.customers
		customers.map((c) => c.Id == store.active.customer ? customer : c)
		setCustomers(customers)
	}

	const updateProject = (project: TProject) => {
		const store = get(customerStore)
		const projects = store.projects
		projects.map((p) => p.Id == store.active.project ? project : p)
		setProjects(projects)
	}


	/**
	 * Update an activity given a already active customer and active project.
	 *
	 * @param {TTask} Task - The new activity data to replace the existing activity with.
	 */
	const updateTask= (task: TTask) => {
		// Retrieve data from the customer store
		const store = get(customerStore);
		const tasks = store.tasks;
		tasks.map((t) => t.Id == store.active.task ? task : t)
		setTasks(tasks)
	}
	

	const addNewTask = (task: TTask) => {
		// Retrieve data from the customer store
		const store = get(customerStore);
		const tasks = [task, ...store.tasks]
		setTasks(tasks)
	};
	
	const addNewCustomer = (newCustomer: TCustomer) => {
		// Retrieve data from the customer store
		const store = get(customerStore);
		const customers = [newCustomer, ...store.customers]
		setCustomers(customers);
	};

	const addNewProject = (newProject: TProject) => {
		// Retrieve data from the customer store
		const store = get(customerStore);
		const projects = [newProject, ...store.projects]
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
		updateCustomer,
		updateProject,
		updateTask,
		addNewTask,
		addNewCustomer,
		addNewProject
	};
}

export const customers = createCustomers();
