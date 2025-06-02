export type TimeEntry = {
	id: number;
  	date: string;
  	value: string;
  	taskId: number;
  	locked: boolean;
  	comment?: string;
  	commentedAt?: string;
}
