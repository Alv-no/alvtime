export type Project = {
	id: string;
	name: string;
	tasks: Task[];
	open: boolean;
	index?: number;
	customer: Customer;
}

export type Customer = {
	name: string;
	lockedTo: Date | null;
}

export type Task = {
	id: string;
	name: string;
	description: string;
	favorite: boolean;
	locked: boolean;
	compensationRate: number;
	enableComments?: boolean;
}