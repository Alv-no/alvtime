import type { TCompensationRate, TCustomer, THourRate, TProject, TTask } from "$lib/types"

export const Customers: TCustomer[] = [
    {
        Id: 1,
        Name: 'Aize',
        InvoiceAddress: 'Fornebu',
        ContactPerson: 'Mr. Aize',
        ContactEmail: 'mraize@aize.aize',
        ContactPhone: '911',
        CustomerNumber: 1,
        Project: [1, 2]
    },
    {
        Id: 2,
        Name: 'Tolletaten',
        InvoiceAddress: 'Svenskegrensen',
        ContactPerson: 'Mr. Toll',
        ContactEmail: 'mrtoll@toll.toll',
        ContactPhone: '911',
        CustomerNumber: 1,
        Project: [3, 4]
    }
]
 

export const Projects: TProject[] = [
    {
        Id: 1,
        Name: 'Super Pipe',
        ProjectNumber: 1,
        StartDate: new Date('December 17, 1995 03:24:00'),
        Customer: 1,
        Task: [1, 2]
    },
    {
        Id: 2,
        Name: 'Building Build',
        ProjectNumber: 2,
        StartDate: new Date('December 17, 1995 03:24:00'),
        Customer: 1,
        Task: [3, 4]
    },
    {
        Id: 3,
        Name: 'Super Toll',
        ProjectNumber: 3,
        StartDate: new Date('December 17, 1995 03:24:00'),
        EndDate: new Date('December 18, 1995 03:24:00'),
        Customer: 2,
        Task: [5, 6]
    },
    {
        Id: 4,
        Name: 'Tolling Build',
        ProjectNumber: 4,
        StartDate: new Date('December 17, 1995 03:24:00'),
        EndDate: new Date('December 18, 1995 03:24:00'),
        Customer: 2,
        Task: [7, 8]
    },

]

export const Tasks: TTask[] = [
    {
        Id: 1,
        Name: 'Design Planning',
        TaskNumber: 1,
        Description: 'Plan the design and stuff',
        StartDate: new Date('December 17, 1995 03:24:00'),
        Project: 1,
        CompensationRate: [1, 2],
        HourRate: [1, 2],
        changedDate: new Date('December 17, 1995 03:24:00')
    },
    {
        Id: 2,
        Name: 'Material Procurement',
        TaskNumber: 1,
        Description: 'Procure the materials and stuff',
        StartDate: new Date('December 17, 1995 03:24:00'),
        EndDate: new Date('December 18, 1995 03:24:00'),
        Project: 1,
        CompensationRate: [3,4],
        HourRate: [3,4],
        changedDate: new Date('December 17, 1995 03:24:00')
    },
    {
        Id: 3,
        Name: 'Design Planning',
        TaskNumber: 2,
        Description: 'Plan the design and stuff',
        StartDate: new Date('December 17, 1995 03:24:00'),
        EndDate: new Date('December 18, 1995 03:24:00'),
        Project: 2,
        CompensationRate: [5, 6],
        HourRate: [5, 6],
        changedDate: new Date('December 17, 1995 03:24:00')
    },
    {
        Id: 4,
        Name: 'Material Procurement',
        TaskNumber: 2,
        Description: 'Procure the materials and stuff',
        StartDate: new Date('December 17, 1995 03:24:00'),
        Project: 2,
        CompensationRate: [7, 8],
        HourRate: [7, 8],
        changedDate: new Date('December 17, 1995 03:24:00')
    },
    {
        Id: 5,
        Name: 'Design Planning',
        TaskNumber: 1,
        Description: 'Plan the design and stuff',
        StartDate: new Date('December 17, 1995 03:24:00'),
        Project: 3,
        CompensationRate: [9, 10],
        HourRate: [9, 10],
        changedDate: new Date('December 17, 1995 03:24:00')
    },
    {
        Id: 6,
        Name: 'Material Procurement',
        TaskNumber: 1,
        Description: 'Procure the materials and stuff',
        StartDate: new Date('December 17, 1995 03:24:00'),
        Project: 3,
        CompensationRate: [11, 12],
        HourRate: [11, 12],
        changedDate: new Date('December 17, 1995 03:24:00')
    },
    {
        Id: 7,
        Name: 'Design Planning',
        TaskNumber: 1,
        Description: 'Plan the design and stuff',
        StartDate: new Date('December 17, 1995 03:24:00'),
        Project: 4,
        CompensationRate: [13, 14],
        HourRate: [13, 14],
        changedDate: new Date('December 17, 1995 03:24:00')
    },
    {
        Id: 8,
        Name: 'Material Procurement',
        TaskNumber: 1,
        Description: 'Procure the materials and stuff',
        StartDate: new Date('December 17, 1995 03:24:00'),
        Project: 4,
        CompensationRate: [15, 16],
        HourRate: [15, 16],
        changedDate: new Date('December 17, 1995 03:24:00')
    },
]

export const CompensationRate: TCompensationRate[] = [
    {
        Id: 1,
        FromDate: new Date('December 17, 1995 03:24:00'),
        Value: 1.5,
        TaskId: 1,
    },
    {
        Id: 2,
        FromDate: new Date('December 17, 1995 03:24:00'),
        Value: 0.5,
        TaskId: 1,
    },
    {
        Id: 3,
        FromDate: new Date('December 17, 1995 03:24:00'),
        Value: 1.5,
        TaskId: 2,
    },
    {
        Id: 4,
        FromDate: new Date('December 17, 1995 03:24:00'),
        Value: 0.5,
        TaskId: 2,
    },
    {
        Id: 5,
        FromDate: new Date('December 17, 1995 03:24:00'),
        Value: 1.5,
        TaskId: 3,
    },
    {
        Id: 6,
        FromDate: new Date('December 17, 1995 03:24:00'),
        Value: 0.5,
        TaskId: 3,
    },
    {
        Id: 7,
        FromDate: new Date('December 17, 1995 03:24:00'),
        Value: 1.5,
        TaskId: 4,
    },
    {
        Id: 8,
        FromDate: new Date('December 17, 1995 03:24:00'),
        Value: 0.5,
        TaskId: 4,
    },
    {
        Id: 9,
        FromDate: new Date('December 17, 1995 03:24:00'),
        Value: 1.5,
        TaskId: 5,
    },
    {
        Id: 10,
        FromDate: new Date('December 17, 1995 03:24:00'),
        Value: 0.5,
        TaskId: 5,
    },
    {
        Id: 11,
        FromDate: new Date('December 17, 1995 03:24:00'),
        Value: 1.5,
        TaskId: 6,
    },
    {
        Id: 12,
        FromDate: new Date('December 17, 1995 03:24:00'),
        Value: 0.5,
        TaskId: 6,
    },
    {
        Id: 13,
        FromDate: new Date('December 17, 1995 03:24:00'),
        Value: 1.5,
        TaskId: 7,
    },
    {
        Id: 14,
        FromDate: new Date('December 17, 1995 03:24:00'),
        Value: 0.5,
        TaskId: 7,
    },
    {
        Id: 15,
        FromDate: new Date('December 17, 1995 03:24:00'),
        Value: 1.5,
        TaskId: 8,
    },
    {
        Id: 16,
        FromDate: new Date('December 17, 1995 03:24:00'),
        Value: 0.5,
        TaskId: 8,
    },
]

export const HourRate: THourRate[] = [
    {
        FromDate: new Date('December 17, 1995 03:24:00'),
        Rate: 1.5,
        TaskId: 1,
        Id: 1,
    },
    {
        FromDate: new Date('December 17, 1995 03:24:00'),
        Rate: 1.5,
        TaskId: 1,
        Id: 2,
    },
    {
        FromDate: new Date('December 17, 1995 03:24:00'),
        Rate: 1.5,
        TaskId: 2,
        Id: 3,
    },
    {
        FromDate: new Date('December 17, 1995 03:24:00'),
        Rate: 1.5,
        TaskId: 2,
        Id: 4,
    },
    {
        FromDate: new Date('December 17, 1995 03:24:00'),
        Rate: 1.5,
        TaskId: 3,
        Id: 5,
    },
    {
        FromDate: new Date('December 17, 1995 03:24:00'),
        Rate: 1.5,
        TaskId: 3,
        Id: 6,
    },
    {
        FromDate: new Date('December 17, 1995 03:24:00'),
        Rate: 1.5,
        TaskId: 4,
        Id: 7,
    },
    {
        FromDate: new Date('December 17, 1995 03:24:00'),
        Rate: 1.5,
        TaskId: 4,
        Id: 8,
    },
    {
        FromDate: new Date('December 17, 1995 03:24:00'),
        Rate: 1.5,
        TaskId: 5,
        Id: 9,
    },
    {
        FromDate: new Date('December 17, 1995 03:24:00'),
        Rate: 1.5,
        TaskId: 5,
        Id: 10,
    },
    {
        FromDate: new Date('December 17, 1995 03:24:00'),
        Rate: 1.5,
        TaskId: 6,
        Id: 11,
    },
    {
        FromDate: new Date('December 17, 1995 03:24:00'),
        Rate: 1.5,
        TaskId: 6,
        Id: 12,
    },
    {
        FromDate: new Date('December 17, 1995 03:24:00'),
        Rate: 1.5,
        TaskId: 7,
        Id: 13,
    },
    {
        FromDate: new Date('December 17, 1995 03:24:00'),
        Rate: 1.5,
        TaskId: 7,
        Id: 14,
    },
    {
        FromDate: new Date('December 17, 1995 03:24:00'),
        Rate: 1.5,
        TaskId: 8,
        Id: 15,
    },
    {
        FromDate: new Date('December 17, 1995 03:24:00'),
        Rate: 1.5,
        TaskId: 8,
        Id: 16,
    }
]
    