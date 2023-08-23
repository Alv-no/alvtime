import type { TActivity, TEmployee, TUser } from "$lib/types";
import { get, writable } from "svelte/store";
import { employees as mock}  from "$lib/mock/employees";

type TEmployeeStore = {
	employees: TUser[],
	active: {
		employee: number | undefined,
	}
}

function createEmployees() {
	const employeeStore = writable<TEmployeeStore>(
		{
			employees: mock,
			active: {
				employee: undefined,
			}
		}
	);
	const { subscribe, set, update } = employeeStore;

	const fetchEmployees = () => {
		//fetch from backend
	}

	const setEmployees = (employees: TUser[]) => {
		update((n) => {n.employees = employees; return n})
		console.log(employees)
	}

    const setEmployee = (Id: number) => {
		if (Id != get(employeeStore).active.employee) {
			update((n) => {n.active.employee = Id; return n})
		}
	}

	const getActiveEmployee = () => {
		const store = get(employeeStore)
		return store.employees.find((e) => e.Id == store.active.employee)
	}

	const updateEmployee = (employee: TUser) => {
		const store = get(employeeStore)
		const employees = store.employees
		employees.map((e) => e.Id == store.active.employee ? employee : e)
		setEmployees(employees)
	}

    const addActivity = (activity: TActivity) => {
        //TODO: add input activity to active employeee
    }

    const removeActivity = (activity: TActivity) => {
        //TODO: remove input activity to active employeee
    }


	return {
		subscribe,
		setEmployees,
        setEmployee,
        getActiveEmployee,
        updateEmployee
	};
}

export const employees = createEmployees();
