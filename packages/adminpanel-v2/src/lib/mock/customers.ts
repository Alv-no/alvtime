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
		startDate: new Date('December 17, 1995 03:24:00'),
		customer: 1,
		changedDate: new Date('December 17, 1995 03:24:00').toLocaleString('en-GB', { timeZone: 'UTC' }).split(',')[0],
		task: [1, 2]
	},
	{
		id: 2,
		name: 'Building Build',
		projectNumber: 2,
		startDate: new Date('December 17, 1995 03:24:00'),
		customer: 1,
		changedDate: new Date('December 17, 1995 03:24:00').toLocaleString('en-GB', { timeZone: 'UTC' }).split(',')[0],
		task: [3, 4]
	},
	{
		id: 3,
		name: 'Super Toll',
		projectNumber: 3,
		startDate: new Date('December 17, 1995 03:24:00'),
		endDate: '2003-04-11',
		customer: 2,
		changedDate: new Date('December 17, 1995 03:24:00').toLocaleString('en-GB', { timeZone: 'UTC' }).split(',')[0],
		task: [5, 6]
	},
	{
		id: 4,
		name: 'Tolling Build',
		projectNumber: 4,
		startDate: new Date('December 17, 1995 03:24:00'),
		endDate: '2003-04-11',
		customer: 2,
		changedDate: new Date('December 17, 1995 03:24:00').toLocaleString('en-GB', { timeZone: 'UTC' }).split(',')[0],
		task: [7, 8]
	}
];

export const Tasks: TTask[] = [
	{
		id: 1,
		name: 'Design Planning',
		taskNumber: 1,
		description: 'Plan the design and stuff',
		startDate: new Date('December 17, 1995 03:24:00'),
		project: 1,
		compensationRate: [1, 2],
		hourRate: [1, 2],
		changedDate: new Date('December 17, 1995 03:24:00').toLocaleString('en-GB', { timeZone: 'UTC' }).split(',')[0]
	},
	{
		id: 2,
		name: 'Material Procurement',
		taskNumber: 1,
		description: 'Procure the materials and stuff',
		startDate: new Date('December 17, 1995 03:24:00'),
		endDate: new Date('December 18, 1995 03:24:00'),
		project: 1,
		compensationRate: [3, 4],
		hourRate: [3, 4],
		changedDate: new Date('December 17, 1995 03:24:00').toLocaleString('en-GB', { timeZone: 'UTC' }).split(',')[0]
	},
	{
		id: 3,
		name: 'Design Planning',
		taskNumber: 2,
		description: 'Plan the design and stuff',
		startDate: new Date('December 17, 1995 03:24:00'),
		endDate: new Date('December 18, 1995 03:24:00'),
		project: 2,
		compensationRate: [5, 6],
		hourRate: [5, 6],
		changedDate: new Date('December 17, 1995 03:24:00').toLocaleString('en-GB', { timeZone: 'UTC' }).split(',')[0]
	},
	{
		id: 4,
		name: 'Material Procurement',
		taskNumber: 2,
		description: 'Procure the materials and stuff',
		startDate: new Date('December 17, 1995 03:24:00'),
		project: 2,
		compensationRate: [7, 8],
		hourRate: [7, 8],
		changedDate: new Date('December 17, 1995 03:24:00').toLocaleString('en-GB', { timeZone: 'UTC' }).split(',')[0]
	},
	{
		id: 5,
		name: 'Design Planning',
		taskNumber: 1,
		description: 'Plan the design and stuff',
		startDate: new Date('December 17, 1995 03:24:00'),
		project: 3,
		compensationRate: [9, 10],
		hourRate: [9, 10],
		changedDate: new Date('December 17, 1995 03:24:00').toLocaleString('en-GB', { timeZone: 'UTC' }).split(',')[0]
	},
	{
		id: 6,
		name: 'Material Procurement',
		taskNumber: 1,
		description: 'Procure the materials and stuff',
		startDate: new Date('December 17, 1995 03:24:00'),
		project: 3,
		compensationRate: [11, 12],
		hourRate: [11, 12],
		changedDate: new Date('December 17, 1995 03:24:00').toLocaleString('en-GB', { timeZone: 'UTC' }).split(',')[0]
	},
	{
		id: 7,
		name: 'Design Planning',
		taskNumber: 1,
		description: 'Plan the design and stuff',
		startDate: new Date('December 17, 1995 03:24:00'),
		project: 4,
		compensationRate: [13, 14],
		hourRate: [13, 14],
		changedDate: new Date('December 17, 1995 03:24:00').toLocaleString('en-GB', { timeZone: 'UTC' }).split(',')[0]
	},
	{
		id: 8,
		name: 'Material Procurement',
		taskNumber: 1,
		description: 'Procure the materials and stuff',
		startDate: new Date('December 17, 1995 03:24:00'),
		project: 4,
		compensationRate: [15, 16],
		hourRate: [15, 16],
		changedDate: new Date('December 17, 1995 03:24:00').toLocaleString('en-GB', { timeZone: 'UTC' }).split(',')[0]
	}
];

export const CompensationRate: TCompensationRate[] = [
	{
		id: 1,
		fromDate: new Date('December 17, 1995 03:24:00'),
		value: 1.5,
		taskId: 1
	},
	{
		id: 2,
		fromDate: new Date('December 17, 1995 03:24:00'),
		value: 0.5,
		taskId: 1
	},
	{
		id: 3,
		fromDate: new Date('December 17, 1995 03:24:00'),
		value: 1.5,
		taskId: 2
	},
	{
		id: 4,
		fromDate: new Date('December 17, 1995 03:24:00'),
		value: 0.5,
		taskId: 2
	},
	{
		id: 5,
		fromDate: new Date('December 17, 1995 03:24:00'),
		value: 1.5,
		taskId: 3
	},
	{
		id: 6,
		fromDate: new Date('December 17, 1995 03:24:00'),
		value: 0.5,
		taskId: 3
	},
	{
		id: 7,
		fromDate: new Date('December 17, 1995 03:24:00'),
		value: 1.5,
		taskId: 4
	},
	{
		id: 8,
		fromDate: new Date('December 17, 1995 03:24:00'),
		value: 0.5,
		taskId: 4
	},
	{
		id: 9,
		fromDate: new Date('December 17, 1995 03:24:00'),
		value: 1.5,
		taskId: 5
	},
	{
		id: 10,
		fromDate: new Date('December 17, 1995 03:24:00'),
		value: 0.5,
		taskId: 5
	},
	{
		id: 11,
		fromDate: new Date('December 17, 1995 03:24:00'),
		value: 1.5,
		taskId: 6
	},
	{
		id: 12,
		fromDate: new Date('December 17, 1995 03:24:00'),
		value: 0.5,
		taskId: 6
	},
	{
		id: 13,
		fromDate: new Date('December 17, 1995 03:24:00'),
		value: 1.5,
		taskId: 7
	},
	{
		id: 14,
		fromDate: new Date('December 17, 1995 03:24:00'),
		value: 0.5,
		taskId: 7
	},
	{
		id: 15,
		fromDate: new Date('December 17, 1995 03:24:00'),
		value: 1.5,
		taskId: 8
	},
	{
		id: 16,
		fromDate: new Date('December 17, 1995 03:24:00'),
		value: 0.5,
		taskId: 8
	}
];

export const HourRate: THourRate[] = [
	{
		fromDate: new Date('December 17, 1995 03:24:00'),
		rate: 1200,
		taskId: 1,
		id: 1
	},
	{
		fromDate: new Date('December 17, 1995 03:24:00'),
		rate: 1200,
		taskId: 1,
		id: 2
	},
	{
		fromDate: new Date('December 17, 1995 03:24:00'),
		rate: 1200,
		taskId: 2,
		id: 3
	},
	{
		fromDate: new Date('December 17, 1995 03:24:00'),
		rate: 1200,
		taskId: 2,
		id: 4
	},
	{
		fromDate: new Date('December 17, 1995 03:24:00'),
		rate: 1200,
		taskId: 3,
		id: 5
	},
	{
		fromDate: new Date('December 17, 1995 03:24:00'),
		rate: 1200,
		taskId: 3,
		id: 6
	},
	{
		fromDate: new Date('December 17, 1995 03:24:00'),
		rate: 1200,
		taskId: 4,
		id: 7
	},
	{
		fromDate: new Date('December 17, 1995 03:24:00'),
		rate: 1200,
		taskId: 4,
		id: 8
	},
	{
		fromDate: new Date('December 17, 1995 03:24:00'),
		rate: 1200,
		taskId: 5,
		id: 9
	},
	{
		fromDate: new Date('December 17, 1995 03:24:00'),
		rate: 1200,
		taskId: 5,
		id: 10
	},
	{
		fromDate: new Date('December 17, 1995 03:24:00'),
		rate: 1200,
		taskId: 6,
		id: 11
	},
	{
		fromDate: new Date('December 17, 1995 03:24:00'),
		rate: 1200,
		taskId: 6,
		id: 12
	},
	{
		fromDate: new Date('December 17, 1995 03:24:00'),
		rate: 1200,
		taskId: 7,
		id: 13
	},
	{
		fromDate: new Date('December 17, 1995 03:24:00'),
		rate: 1200,
		taskId: 7,
		id: 14
	},
	{
		fromDate: new Date('December 17, 1995 03:24:00'),
		rate: 1200,
		taskId: 8,
		id: 15
	},
	{
		fromDate: new Date('December 17, 1995 03:24:00'),
		rate: 1200,
		taskId: 8,
		id: 16
	}
];
