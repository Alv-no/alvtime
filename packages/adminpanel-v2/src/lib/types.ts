export type TActivity = {id: number, name: string, price?: number, changedDate?: Date}

export type TProject = {id: number, name: string, activities: TActivity[]}

export type TCustomer = {id: number, name: string, projects: TProject[]}



