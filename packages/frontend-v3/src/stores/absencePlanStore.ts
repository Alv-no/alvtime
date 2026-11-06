import { defineStore } from "pinia";
import { ref } from "vue";
import absenceService from "@/services/absenceService";
import type { AbsencePlan, AbsencePlanParams, AbsencePlanTask } from "@/types/AbsencePlanTypes";
import { useVacationStore } from "./vacationStore";
import { useTimeBankStore } from "./timeBankStore";
import { useTimeEntriesStore } from "./timeEntriesStore";

const extractErrorMessages = (error: unknown): string[] => {
	const data = (error as { response?: { data?: { title?: string; errors?: Record<string, string[]> } } })
		?.response?.data;
	const messages = data?.errors ? Object.values(data.errors).flat() : [];
	if (messages.length > 0) return messages;
	return [data?.title ?? "Noe gikk galt. Prøv igjen."];
};

export const useAbsencePlanStore = defineStore("absencePlan", () => {
	const tasks = ref<AbsencePlanTask[]>([]);

	const getTasks = async () => {
		if (tasks.value.length > 0) return;
		const response = await absenceService.getAbsencePlanTasks();
		tasks.value = response.data;
	};

	const getPlan = async (params: AbsencePlanParams): Promise<{ plan?: AbsencePlan; errors?: string[] }> => {
		try {
			const response = await absenceService.getAbsencePlan(params);
			return { plan: response.data };
		} catch (error) {
			return { errors: extractErrorMessages(error) };
		}
	};

	const registerPlan = async (params: AbsencePlanParams): Promise<{ plan?: AbsencePlan; errors?: string[] }> => {
		let plan: AbsencePlan;
		try {
			const response = await absenceService.registerAbsencePlan(params);
			plan = response.data;
		} catch (error) {
			return { errors: extractErrorMessages(error) };
		}

		// Lagringen er vellykket selv om en oppfriskning feiler
		await Promise.allSettled([
			useVacationStore().getVacationOverview(),
			useTimeBankStore().getTimeBankOverview(),
			useTimeEntriesStore().getTimeEntries({
				fromDateInclusive: params.fromDateInclusive,
				toDateInclusive: params.toDateInclusive,
			}),
		]);
		return { plan };
	};

	return { tasks, getTasks, getPlan, registerPlan };
});
