export type TActivity = {id: number, name: string, aktivitetnummer: number, price?: number, changedDate?: Date}

export type TProject = {id: number, name: string, prosjektnummer: number, activities: TActivity[]}

export type TCustomer = {id: number, name: string, kundenummer: number, projects: TProject[]}



