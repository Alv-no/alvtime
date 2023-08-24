export type TCompensationRate = {
	id: number;
	fromDate: Date;
	value: number; //Skal være desimaltall, er dette number i js?
	taskId: number;
};

export type THourRate = {
	fromDate: Date;
	rate: number; //desimal
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
};

export type TProject = {
	id: number;
	name: string;
	customer: number;
	projectNumber?: number;
	task: number[];
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
	endDate?: Date;
	employeeId: number;
};
