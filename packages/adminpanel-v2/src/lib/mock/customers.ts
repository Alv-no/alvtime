import type { TCompensationRate, TCustomer, THourRate, TProject, TTask } from '$lib/types';

export const Customers: TCustomer[] = [
	{
		id: 1,
		name: 'Aize',
		invoiceAddress: 'Fornebu',
		contactPerson: 'Mr. Aize',
		contactEmail: 'mraize@aize.aize',
		contactPhone: '911',
		customerNumber: 1,
		project: [1, 2]
	},
	{
		id: 2,
		name: 'Tolletaten',
		invoiceAddress: 'Svenskegrensen',
		contactPerson: 'Mr. Toll',
		contactEmail: 'mrtoll@toll.toll',
		contactPhone: '911',
		customerNumber: 1,
		project: [3, 4]
	}
];

export const Projects: TProject[] = [
	{
		id: 1,
		name: 'Super Pipe',
		projectNumber: 1,
		customer: 1,
		task: [1, 2]
	},
	{
		id: 2,
		name: 'Building Build',
		projectNumber: 1,
		customer: 1,
		task: [3, 4]
	},
	{
		id: 3,
		name: 'Super Toll',
		projectNumber: 1,
		customer: 2,
		task: [5, 6]
	},
	{
		id: 4,
		name: 'Tolling Build',
		projectNumber: 1,
		customer: 2,
		task: [7, 8]
	}
];

export const Tasks: TTask[] = [
	{
		id: 1,
		name: 'Design Planning',
		taskNumber: 1,
		description: 'Plan the design and stuff',
		project: 1,
		compensationRate: [1, 2],
		hourRate: [1, 2],
		changedDate: new Date()
	},
	{
		id: 2,
		name: 'Material Procurement',
		taskNumber: 1,
		description: 'Procure the materials and stuff',
		project: 1,
		compensationRate: [3, 4],
		hourRate: [3, 4],
		changedDate: new Date()
	},
	{
		id: 3,
		name: 'Design Planning',
		taskNumber: 2,
		description: 'Plan the design and stuff',
		project: 1,
		compensationRate: [5, 6],
		hourRate: [5, 6],
		changedDate: new Date()
	},
	{
		id: 4,
		name: 'Material Procurement',
		taskNumber: 2,
		description: 'Procure the materials and stuff',
		project: 1,
		compensationRate: [7, 8],
		hourRate: [7, 8],
		changedDate: new Date()
	},
	{
		id: 5,
		name: 'Design Planning',
		taskNumber: 1,
		description: 'Plan the design and stuff',
		project: 3,
		compensationRate: [9, 10],
		hourRate: [9, 10],
		changedDate: new Date()
	},
	{
		id: 6,
		name: 'Material Procurement',
		taskNumber: 1,
		description: 'Procure the materials and stuff',
		project: 3,
		compensationRate: [11, 12],
		hourRate: [11, 12],
		changedDate: new Date()
	},
	{
		id: 7,
		name: 'Design Planning',
		taskNumber: 1,
		description: 'Plan the design and stuff',
		project: 4,
		compensationRate: [13, 14],
		hourRate: [13, 14],
		changedDate: new Date()
	},
	{
		id: 8,
		name: 'Material Procurement',
		taskNumber: 1,
		description: 'Procure the materials and stuff',
		project: 4,
		compensationRate: [15, 16],
		hourRate: [15, 16],
		changedDate: new Date()
	}
];

export const CompensationRate: TCompensationRate[] = [
	{
		id: 1,
		fromDate: new Date(),
		value: 1.5,
		taskId: 1
	},
	{
		id: 2,
		fromDate: new Date(),
		value: 0.5,
		taskId: 1
	},
	{
		id: 3,
		fromDate: new Date(),
		value: 1.5,
		taskId: 2
	},
	{
		id: 4,
		fromDate: new Date(),
		value: 0.5,
		taskId: 2
	},
	{
		id: 5,
		fromDate: new Date(),
		value: 1.5,
		taskId: 3
	},
	{
		id: 6,
		fromDate: new Date(),
		value: 0.5,
		taskId: 3
	},
	{
		id: 7,
		fromDate: new Date(),
		value: 1.5,
		taskId: 4
	},
	{
		id: 8,
		fromDate: new Date(),
		value: 0.5,
		taskId: 4
	},
	{
		id: 9,
		fromDate: new Date(),
		value: 1.5,
		taskId: 5
	},
	{
		id: 10,
		fromDate: new Date(),
		value: 0.5,
		taskId: 5
	},
	{
		id: 11,
		fromDate: new Date(),
		value: 1.5,
		taskId: 6
	},
	{
		id: 12,
		fromDate: new Date(),
		value: 0.5,
		taskId: 6
	},
	{
		id: 13,
		fromDate: new Date(),
		value: 1.5,
		taskId: 7
	},
	{
		id: 14,
		fromDate: new Date(),
		value: 0.5,
		taskId: 7
	},
	{
		id: 15,
		fromDate: new Date(),
		value: 1.5,
		taskId: 8
	},
	{
		id: 16,
		fromDate: new Date(),
		value: 0.5,
		taskId: 8
	}
];

export const HourRate: THourRate[] = [
	{
		fromDate: new Date(),
		rate: 1.5,
		taskId: 1,
		id: 1
	},
	{
		fromDate: new Date(),
		rate: 1.5,
		taskId: 1,
		id: 2
	},
	{
		fromDate: new Date(),
		rate: 1.5,
		taskId: 2,
		id: 3
	},
	{
		fromDate: new Date(),
		rate: 1.5,
		taskId: 2,
		id: 4
	},
	{
		fromDate: new Date(),
		rate: 1.5,
		taskId: 3,
		id: 5
	},
	{
		fromDate: new Date(),
		rate: 1.5,
		taskId: 3,
		id: 6
	},
	{
		fromDate: new Date(),
		rate: 1.5,
		taskId: 4,
		id: 7
	},
	{
		fromDate: new Date(),
		rate: 1.5,
		taskId: 4,
		id: 8
	},
	{
		fromDate: new Date(),
		rate: 1.5,
		taskId: 5,
		id: 9
	},
	{
		fromDate: new Date(),
		rate: 1.5,
		taskId: 5,
		id: 10
	},
	{
		fromDate: new Date(),
		rate: 1.5,
		taskId: 6,
		id: 11
	},
	{
		fromDate: new Date(),
		rate: 1.5,
		taskId: 6,
		id: 12
	},
	{
		fromDate: new Date(),
		rate: 1.5,
		taskId: 7,
		id: 13
	},
	{
		fromDate: new Date(),
		rate: 1.5,
		taskId: 7,
		id: 14
	},
	{
		fromDate: new Date(),
		rate: 1.5,
		taskId: 8,
		id: 15
	},
	{
		fromDate: new Date(),
		rate: 1.5,
		taskId: 8,
		id: 16
	}
];
