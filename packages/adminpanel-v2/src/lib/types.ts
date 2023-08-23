export type TActivity = {
    id: number, 
    name: string, 
    aktivitetnummer?: number,
    price?: number, 
    changedDate?: Date,
    overtimeFactor?: number
}

export type TProject = {id: number, name: string, prosjektnummer: number, activities: TActivity[]}

export type TCustomer = {id: number, name: string, kundenummer: number, projects: TProject[]}

export type TEmployee = {id: number, name: string, ansattnummer: number, activities: TActivity[]}


//Types from backend

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
    //Project: number (samme som customer i TProject)
    CompensationRate: TCompensationRate[]
    HourRate: THourRate[]
    changedDate?: Date}

export type TProjectBackend = {
    Id: number,
    Name: string,
    //customer: number, (trengs ikke fordi store er nøstet?)
    pprojectNumber?: number,
    Task: TTask[]
}

export type TCustomerBackend = {
    id: number,
    name: string,
    InvoiceAddress: string,
    ContactPerson: string,
    ContactEmail: string,
    ContactPhone: string,
    CustomerNumber?: number,
    Project: TProject[],
}

export type TUser = {
    Id: number,
    Name: string,
    Email: string,
    StartDate: Date,
    EndDate?: Date,
    EmployeeId: number,
}



