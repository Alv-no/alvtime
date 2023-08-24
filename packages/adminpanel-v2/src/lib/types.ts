export type TCompensationRate = {
    Id: number,
    FromDate: Date,
    Value: number,
    TaskId: number,
}

export type THourRate = {
    FromDate: Date,
    Rate: number,
    TaskId: number,
    Id: number,
}

export type TTask = {
    Id: number,
    Name: string,
    TaskNumber?: number,
    Description: string,
    StartDate?: Date,
    EndDate?: Date,
    Project: number,
    CompensationRate: number[]
    HourRate: number[]
    changedDate?: Date}

export type TProject = {
    Id: number,
    Name: string,
    Customer: number,
    ProjectNumber?: number,
    StartDate?: Date,
    EndDate?: Date,
    Task: number[]
}

export type TCustomer = {
    Id: number,
    Name: string,
    InvoiceAddress: string,
    ContactPerson: string,
    ContactEmail: string,
    ContactPhone: string,
    CustomerNumber?: number,
    Project: number[],
}

export type TUser = {
    Id: number,
    Name: string,
    Email: string,
    StartDate: Date,
    EndDate?: Date,
    EmployeeId: number,
}



