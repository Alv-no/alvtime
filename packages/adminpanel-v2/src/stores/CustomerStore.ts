//import { TCustomer, TProject, TActivity} from "$lib/types"
import type { TCustomer, TProject } from "$lib/types";
import { get, writable } from "svelte/store";
import { customers as mock}  from "$lib/mock/customers";

type TCustomerStore = {
	customers: TCustomer[],
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
		console.log(customers)
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

	const setAvtivity = (id: number) => {
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


	return {
		subscribe,
		setCustomers,
        setCustomer,
		setProject,
		setAvtivity,
		getActiveCustomer,
		getActiveProject,
		getActiveActivity,
		updateCustomer,
		updateProject,
	};
}

export const customers = createCustomers();
