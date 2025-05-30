export type Project = {
  id: string;
  name: string;
  customer: Customer;
  tasks: Task[];
}


export type Customer = {
	id: string;
	name: string;
	orgNr?: string;
	contactEmail?: string;
	contactPhone?: string;
	contactPerson?: string;
	invoiceAddress?: string;	
}

export type Task = {
	id: string;
	name: string;
	description: string;
	favorite: boolean;
	locked: boolean;
	compensationRate: number;
}