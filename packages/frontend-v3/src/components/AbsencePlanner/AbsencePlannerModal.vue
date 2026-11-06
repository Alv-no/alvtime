<template>
	<ModalComponent
		title="Registrer fravær"
		@close="emit('close')"
	>
		<form
			class="absence-planner"
			@submit.prevent="register"
		>
			<div class="input-group">
				<label for="absence-task">Type</label>
				<select
					id="absence-task"
					v-model.number="taskId"
				>
					<option
						v-for="task in tasks"
						:key="task.id"
						:value="task.id"
					>
						{{ task.name }}
					</option>
				</select>
			</div>
			<div class="input-group">
				<label for="absence-period">Periode</label>
				<VueDatePicker
					id="absence-period"
					v-model="period"
					:range="{ maxRange: 366 }"
					:timeConfig="{ enableTimePicker: false }"
					:formats="{ input: 'dd.MM.yyyy' }"
					:weekNumbers="{ type: 'iso' }"
					:clearable="false"
					:teleport="true"
					autoApply
					placeholder="Velg fra- og til-dato"
				/>
			</div>
			<div class="input-group">
				<label for="absence-percentage">Andel av dagen (%)</label>
				<input
					id="absence-percentage"
					v-model.number="percentage"
					class="percentage-input"
					type="number"
					min="1"
					max="100"
					step="1"
				/>
			</div>
			<div class="checkbox-group">
				<input
					id="absence-fill-partial"
					v-model="fillPartialDays"
					type="checkbox"
				/>
				<label for="absence-fill-partial">Fyll opp dager som har andre timer</label>
			</div>

			<div class="preview">
				<p v-if="!params">
					Velg type og periode for å se hvilke dager som registreres.
				</p>
				<p v-else-if="loadingPlan && !plan">
					Henter forhåndsvisning …
				</p>
				<template v-else-if="plan">
					<AbsencePlanSummary
						:plan="plan"
						:availableVacationDays="availableVacationDays"
					/>
					<AbsencePlanSkippedDays :days="plan.days" />
					<AbsencePlanDayList :days="plan.days" />
				</template>
				<ErrorBox v-if="errors.length > 0">
					<p
						v-for="error in errors"
						:key="error"
					>
						{{ error }}
					</p>
				</ErrorBox>
			</div>

			<div class="input-group">
				<label for="absence-comment">Kommentar (valgfri)</label>
				<textarea
					id="absence-comment"
					v-model="comment"
					class="comment-input"
					rows="4"
				/>
			</div>

			<div class="actions">
				<AlvtimeButton
					type="button"
					linkButton
					@click="emit('close')"
				>
					Avbryt
				</AlvtimeButton>
				<AlvtimeButton
					type="submit"
					:disabled="!canRegister"
				>
					{{ registerLabel }}
				</AlvtimeButton>
			</div>
		</form>
	</ModalComponent>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from "vue";
import { storeToRefs } from "pinia";
import { VueDatePicker } from "@vuepic/vue-datepicker";
import "@vuepic/vue-datepicker/dist/main.css";
import ModalComponent from "@/components/utils/ModalComponent.vue";
import AlvtimeButton from "@/components/utils/AlvtimeButton.vue";
import ErrorBox from "@/components/utils/ErrorBox.vue";
import AbsencePlanSummary from "./AbsencePlanSummary.vue";
import AbsencePlanSkippedDays from "./AbsencePlanSkippedDays.vue";
import AbsencePlanDayList from "./AbsencePlanDayList.vue";
import { useAbsencePlanStore } from "@/stores/absencePlanStore";
import { useVacationStore } from "@/stores/vacationStore";
import { debounce } from "@/utils/generalHelpers";
import type { AbsencePlan, AbsencePlanParams } from "@/types/AbsencePlanTypes";

const emit = defineEmits<{
	(event: "close"): void;
	(event: "registered", plan: AbsencePlan): void;
}>();

const absencePlanStore = useAbsencePlanStore();
const vacationStore = useVacationStore();
const { tasks } = storeToRefs(absencePlanStore);
const { vacation } = storeToRefs(vacationStore);

const taskId = ref<number>();
const period = ref<Date[] | null>(null);
const percentage = ref<number>(100);
const fillPartialDays = ref(false);
const comment = ref("");

const plan = ref<AbsencePlan | null>(null);
const loadingPlan = ref(false);
const saving = ref(false);
const errors = ref<string[]>([]);

const selectedTask = computed(() => tasks.value.find(t => t.id === taskId.value));

const params = computed<AbsencePlanParams | null>(() => {
	const [from, to] = period.value ?? [];
	if (!taskId.value || !from || !to) return null;
	return {
		taskId: taskId.value,
		fromDateInclusive: from,
		toDateInclusive: to,
		percentage: percentage.value,
		fillPartialDays: fillPartialDays.value,
	};
});

const availableVacationDays = computed(() =>
	selectedTask.value?.isPaidHoliday ? vacation.value?.availableVacationDays : undefined
);

const canRegister = computed(() =>
	!!params.value && !!plan.value && plan.value.totalDays > 0 && !loadingPlan.value && !saving.value
);

const registerLabel = computed(() => {
	if (saving.value) return "Registrerer …";
	const days = plan.value?.totalDays ?? 0;
	return days === 1 ? "Registrer 1 dag" : `Registrer ${days} dager`;
});

// Forkaster svar som kommer etter at et nyere kall er startet
let latestRequestId = 0;

const fetchPlan = async () => {
	const currentParams = params.value;
	if (!currentParams) return;
	const requestId = ++latestRequestId;
	const result = await absencePlanStore.getPlan(currentParams);
	if (requestId !== latestRequestId) return;
	plan.value = result.plan ?? null;
	errors.value = result.errors ?? [];
	loadingPlan.value = false;
};

const debouncedFetchPlan = debounce(fetchPlan, 400);

watch(params, (newParams) => {
	errors.value = [];
	if (!newParams) {
		latestRequestId++;
		plan.value = null;
		loadingPlan.value = false;
		return;
	}
	loadingPlan.value = true;
	debouncedFetchPlan();
});

const register = async () => {
	if (!params.value || !canRegister.value) return;
	saving.value = true;
	const result = await absencePlanStore.registerPlan({ ...params.value, comment: comment.value || undefined });
	saving.value = false;
	if (!result.plan) {
		errors.value = result.errors ?? [];
		return;
	}
	emit("registered", result.plan);
	emit("close");
};

onMounted(async () => {
	try {
		await absencePlanStore.getTasks();
	} catch {
		errors.value = ["Klarte ikke å hente fraværskoder."];
		return;
	}
	taskId.value = tasks.value[0]?.id;
	if (!vacation.value) {
		vacationStore.getVacationOverview().catch(() => undefined);
	}
});
</script>

<style scoped lang="scss">
.absence-planner {
	display: flex;
	flex-direction: column;
	max-width: 560px;
}

.input-group {
	display: flex;
	flex-direction: column;
	margin-bottom: 16px;

	label {
		font-weight: 600;
		margin-bottom: 8px;
	}

	input,
	select,
	textarea {
		padding: 8px;
		border: 1px solid #ccc;
		border-radius: 4px;
		font-size: 16px;
	}

	.percentage-input {
		width: 120px;
	}

	.comment-input {
		width: 100%;
		box-sizing: border-box;
		background-color: rgb(250, 245, 235);
		font-family: inherit;
		resize: vertical;
	}
}

.checkbox-group {
	display: flex;
	align-items: center;
	gap: 8px;
	margin-bottom: 16px;
}

.preview {
	border-top: 1px solid $secondary-color;
	border-bottom: 1px solid $secondary-color;
	padding: 12px 0;
	margin-bottom: 16px;
}

.actions {
	display: flex;
	justify-content: flex-end;
	gap: 8px;
}
</style>
