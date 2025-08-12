export type TimeEntry = {
	id: string;
  	date: string;
  	value: number;
  	taskId: string;
  	locked: boolean;
  	comment?: string;
  	commentedAt?: Date;
}

export type TimeEntryMap = {
	[key: string]: TimeEntryObj;
};

export type TimeEntryObj = {
	id: string;
  	value:  number;
  	comment?: string;
};
