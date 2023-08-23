//import { TCustomer, TProject, TActivity} from "$lib/types"
import type { TActivity, TCustomer, TProject } from "$lib/types";
import { get, writable } from "svelte/store";
import { customers as mock}  from "$lib/mock/customersOld";

type TCustomerStore = {
	customers: TCustomer[],
	//TODO: Update store object structure to match backend
	//projects: TProject[],
	//tasks: TTask[],
	active: {
		customer: number | undefined,
		project: number | undefined,
		activity: number | undefined
	}
}

function createCustomers() {
	const customerStore = writable<TCustomerStore>(
		{
			customers: mock,
			active: {
				customer: undefined,
				project: undefined,
				activity: undefined
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

    const setCustomer = (id: number) => {
		if (id != get(customerStore).active.customer) {
			update((n) => {n.active.customer = id; return n})
			update((n) => {n.active.project = undefined; return n})
			update((n) => {n.active.activity = undefined; return n})
		}
	}

	const setProject = (id: number) => {
		if (id != get(customerStore).active.project) {
			update((n) => {n.active.project = id; return n})
			update((n) => {n.active.activity = undefined; return n})
		}
	}

	const setActivity = (id: number) => {
		update((n) => {n.active.activity = id; return n})
	}

	const getActiveCustomer = () => {
		const store = get(customerStore)
		return store.customers.find((c) => c.id == store.active.customer)
	}

	const getActiveProject = () => {
		const store = get(customerStore)
		return store.customers.find((c) => c.id == store.active.customer)?.projects.find((p) => p.id == store.active.project)
	}

	const getActiveActivity = () => {
		const store = get(customerStore)
		return store.customers.find((c) => c.id == store.active.customer)?.projects.find((p) => p.id == store.active.project)?.activities.find((a) => a.id == store.active.activity)
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
		customers.map((c) => c.id == store.active.customer ? customer : c)
		setCustomers(customers)
	}

	const updateProject = (project: TProject) => {
		const store = get(customerStore)
		const customers = store.customers
		customers.map((c) => c.id == store.active.customer?  c.projects.map((p) => p.id == store.active.project ? project : p) : c)
		setCustomers(customers)
	}

	

	// HELPER FUNCTIONS FOR updateActivity
	const _handleSingleCustomerUpdate = (targetCustomerId: number, targetProjectId: number, targetActivityId: number, newActivity: TActivity) => (customer: TCustomer) => {
		if (customer.id !== targetCustomerId) return customer;

		// Update projects within the customer
		const updatedProjects = customer.projects.map(_handleSingleProjectUpdate(targetProjectId, targetActivityId, newActivity));
		return { ...customer, projects: updatedProjects };
	};

	const _handleSingleProjectUpdate = (targetProjectId: number, targetActivityId: number, newActivity: TActivity) => (project: TProject) => {
		if (project.id !== targetProjectId) return project;

		// Update activities within the project
		const updatedActivities = project.activities.map(_handleSingleActivityUpdate(targetActivityId, newActivity));
		return { ...project, activities: updatedActivities };
		};

	const _handleSingleActivityUpdate = (targetActivityId: number, newActivity: TActivity) => (act: TActivity) => {
		if (act.id !== targetActivityId) return act;
		return { ...newActivity, id: targetActivityId };
	};



	/**
	 * Update an activity given a already active customer and active project.
	 *
	 * @param {TActivity} newActivity - The new activity data to replace the existing activity with.
	 */
	const updateActivity = (newActivity: TActivity) => {
		// Retrieve data from the customer store
		const store = get(customerStore);
		const customers = store.customers;
		const { customer, project, activity } = store.active;

		// Check if all necessary data is available for update
		if (customer !== undefined && project !== undefined && activity !== undefined) {
			// Update customers based on the provided information
			const updatedCustomers = customers.map(_handleSingleCustomerUpdate(customer, project, activity, newActivity));
			setCustomers(updatedCustomers);
		}
	};
	
	// HELPER FUNCTIONS addNewActivity
	const _handleSingleCustomerAddNew = (targetCustomerId: number, targetProjectId: number, newActivity: TActivity) => (customer: TCustomer) => {
		if (customer.id !== targetCustomerId) return customer;

		// Update projects within the customer
		const updatedProjects = customer.projects.map(_handleSingleProjectAddNew(targetProjectId, newActivity));
		return { ...customer, projects: updatedProjects };
	};

	const _handleSingleProjectAddNew = (targetProjectId: number, newActivity: TActivity) => (project: TProject) => {
		if (project.id !== targetProjectId) return project;

		// Update activities within the project
		project.activities.unshift(newActivity);
		return { ...project, activities: project.activities};
	};

	const addNewActivity = (newActivity: TActivity) => {
		// Retrieve data from the customer store
		const store = get(customerStore);
		const customers = store.customers;

		// Check if all necessary data is available for update
		if (store.active.customer !== undefined && store.active.project !== undefined ) {
			// Update customers based on the provided information
			const updatedCustomers = customers.map(_handleSingleCustomerAddNew(store.active.customer, store.active.project,  newActivity));
			setCustomers(updatedCustomers);
		}
	};
	
	
	


	return {
		subscribe,
		setCustomers,
        setCustomer,
		setProject,
		setActivity,
		getActiveCustomer,
		getActiveProject,
		getActiveActivity,
		updateCustomer,
		updateProject,
		updateActivity,
		addNewActivity
	};
}

export const customers = createCustomers();
