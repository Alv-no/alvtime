export type TCompensationRate = {
	id: number;
	fromDate: Date;
	endDate?: Date;
	value: number;
	taskId: number;
};

export type THourRate = {
	fromDate: Date;
	endDate?: Date;
	rate: number;
	taskId: number;
	id: number;
};

export type TTask = {
	id: number;
	name: string;
	taskNumber?: number;
	description: string;
	project: number;
	compensationRate: number[];
	hourRate: number[];
	changedDate?: Date;
	startDate?: Date;
	endDate?: Date;
};

export type TProject = {
	id: number;
	name: string;
	customer: number;
	projectNumber?: number;
	task: number[];
	startDate?: Date;
	endDate?: string;
};

export type TCustomer = {
	id: number;
	name: string;
	invoiceAddress: string;
	contactPerson: string;
	contactEmail: string;
	contactPhone: string;
	customerNumber?: number;
	project: number[];
};

export type TUser = {
	id: number;
	name: string;
	email: string;
	startDate: Date;
	endDate?: string;
	employeeId: number;
};
