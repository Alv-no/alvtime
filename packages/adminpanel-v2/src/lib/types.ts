export type TCompensationRate = {
    Id: number,
    FromDate: Date,
    Value: number, //Skal være desimaltall, er dette number i js?
    TaskId: number,
}

export type THourRate = {
    FromDate: Date,
    Rate: number, //desimal
    TaskId: number,
    Id: number,
}

export type TTask = {
    Id: number,
    Name: string,
    TaskNumber?: number,
    Description: string,
    Project: number,
    CompensationRate: number[]
    HourRate: number[]
    changedDate?: Date}

export type TProject = {
    Id: number,
    Name: string,
    Customer: number,
    ProjectNumber?: number,
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



