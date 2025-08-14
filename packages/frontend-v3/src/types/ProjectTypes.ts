export type Project = {
  id: string;
  name: string;
  customerName: string;
  tasks: Task[];
  open: boolean;
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