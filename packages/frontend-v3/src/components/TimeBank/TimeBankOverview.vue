<template v-if="!loading">
	<div class="header-flex-container">
		<h2>
			Overtidstimer
			<div
				v-if="userProfile?.salaryModel === 1"
				class="tooltip"
			>
				&#9432;
				<span class="tooltiptext">
					Med lønn med faktureringsledd er de første 50 timene med interntid- og frivillig
					overtid hvert år inkludert i lønnen din. Du må tjene disse opp før timene begynner
					å spare i timebanken. Fakturerbare timer gir deg en bonus basert på
					faktureringsgraden din.
				</span>
			</div>
		</h2>
		<button @click="settingsModalOpen = true">
			<FeatherIcon
				name="settings"
				:size="18"
			/>
		</button>
		<span v-if="timeBankSalary && timeBankSalary > 0">
			Kroneverdi timebank: {{ timeBankValue }}
		</span>
	</div>
	<span v-if="userProfile">
		<p>Din lønnsmodell er: {{ salaryModelText }}</p>
	</span>
	<div v-if="noOvertime">
		<p>Du har ingen overtidstimer i timebanken.</p>
	</div>
	<template v-else>
		<SectionedBar :sections="overtimeSections" />
		<TimeBankPayoutForm />
	</template>
	<ErrorBox
		v-if="timeBankError.status"
		:closable="true"
		@close="timeBankError = {}"
	>
		<p>{{ timeBankError.errors?.InvalidAction[0] || timeBankError.errors }}</p>
	</ErrorBox>
	<TimeBankHistory />
	<ModalComponent
		v-if="settingsModalOpen"
		title="Innstillinger for timebank"
		:clickOutsideToClose="true"
		@close="() => { settingsModalOpen = false; }"
	>
		<div class="input-group">
			<label for="salary-input">Årslønn (lagres kun lokalt og brukes til å beregne og visualisere verdi av timebank)</label>
			<input
				id="salary-input"
				v-model="salary"
				type="number"
				placeholder="Skriv inn årslønn"
			/>
		</div>
		<button
			class="button"
			@click="settingsModalOpen = false"
		>
			Lagre og lukk
		</button>
	</ModalComponent>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from "vue";
import { storeToRefs } from "pinia";
import SectionedBar from "./SectionedBar.vue";
import { useTimeBankStore } from "@/stores/timeBankStore";
import TimeBankPayoutForm from "./TimeBankPayoutForm.vue";
import TimeBankHistory from "./TimeBankHistory.vue";
import ErrorBox from "../utils/ErrorBox.vue";
import FeatherIcon from "@/components/utils/FeatherIcon.vue";
import ModalComponent from "../utils/ModalComponent.vue";
import { useUserStore } from "@/stores/userStore.ts";

const loading = ref<boolean>(true);
const settingsModalOpen = ref<boolean>(false);

const timeBankStore = useTimeBankStore();
const { timeBankOverview, timeBankError, timeBankSalary } = storeToRefs(timeBankStore);

const userStore = useUserStore();
const { userProfile } = storeToRefs(userStore);

const salaryModelMap: Record<number, string> = {
	0: "Fastlønn",
	1: "Lønn med faktureringsledd",
};

const salaryModelText = computed(() => {
	const salaryModel = userProfile.value?.salaryModel;

	return salaryModel === undefined
		? ""
		: salaryModelMap[salaryModel] ?? "Ukjent lønnsmodell";
});

const salary = computed({
	get: () => timeBankSalary.value,
	set: (value: number) => {
		timeBankStore.setTimeBankSalary(value || null);
	}
});

const hourlyRate = computed(() => {
	if (!timeBankSalary.value) return 0;
	return timeBankSalary.value / 1950; // Assuming 1950 working hours per year
});

const timeBankValue = computed(() => {
	if (!timeBankOverview.value) return "0";
	const totalHours = timeBankOverview.value.entries.reduce((acc, entry) => acc + (entry.hours * entry.compensationRate), 0);
	const value = Math.floor(totalHours * hourlyRate.value);
	return value.toString().replace(/\B(?=(\d{3})+(?!\d))/g, " ") + ",-";
});

const overtimeSections = computed(() => {
	const unspentOverTime = {
		volunteer: timeBankOverview.value?.entries.filter(entry => entry.compensationRate === 0.5).reduce((acc, entry) => acc + entry.hours, 0) || 0,
		mandatory: timeBankOverview.value?.entries.filter(entry => entry.compensationRate === 1).reduce((acc, entry) => acc + entry.hours, 0) || 0,
		billable: timeBankOverview.value?.entries.filter(entry => entry.compensationRate === 1.5 || entry.compensationRate === 1.4).reduce((acc, entry) => acc + entry.hours, 0) || 0,
		mandatoryBillable: timeBankOverview.value?.entries.filter(entry => entry.compensationRate === 2).reduce((acc, entry) => acc + entry.hours, 0) || 0,
	};

	return [
		{
			title: "Frivillig",
			amount: unspentOverTime.volunteer,
			color: "yellow",
		},
		{
			title: "Interntid",
			amount: unspentOverTime.mandatory,
			color: "blue",
		},
		{
			title: "Fakturerbart",
			amount: unspentOverTime.billable,
			color: "green",
		},
		{
			title: "Tommy Time",
			amount: unspentOverTime.mandatoryBillable,
			color: "red",
		}
	];
});

const noOvertime = computed(() => {
	return overtimeSections.value.every(section => section.amount === 0);
});

onMounted(async () => {
	await timeBankStore.getTimeBankOverview();
	await userStore.getUserProfile();
	loading.value = false;
});

</script>

<style scoped lang="scss">
.tooltip {
	position: relative;
	display: inline-block;
	border-bottom: 1px dotted $primary-color;
	cursor: pointer;
	vertical-align: middle;
	font-size: 0.9em;
}

.tooltiptext {
	visibility: hidden;
	width: 400px;
	background-color: $primary-color;
	color: $background-color;
	padding: 16px;
	border-radius: 10px;
	position: absolute;
	z-index: 1;
	font-family: system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif;
	font-size: 14px;
	line-height: 1.65;
	box-shadow: 0 8px 24px rgba(0, 0, 0, 0.2);

	p { margin: 12px 0; }
}

.tooltip:hover .tooltiptext {
	visibility: visible;
}

.header-flex-container {
	display: flex;
	align-items: baseline;
	justify-content: flex-start;
	gap: 8px;

	.header-title {
		display: flex;
		align-items: baseline;
		gap: 8px;
	}

	h2 {
		margin-top: 32px;
		margin-bottom: 8px;
	}

	button {
		position: relative;
		background: none;
		border: none;
		cursor: pointer;
		padding: 0;
	}
}

.input-group {
	display: flex;
	flex-direction: column;
	margin-bottom: 16px;

	label {
		font-weight: 600;
		margin-bottom: 8px;
	}

	input {
		padding: 8px;
		border: 1px solid #ccc;
		border-radius: 4px;
		font-size: 16px;
		width: 90%;;
	}
}

.button {
	background-color: $secondary-color;
	color: $primary-color;
	border-radius: 25px;
	border: none;
	padding: 13px 24px 13px 24px;
	cursor: pointer;
	font-size: 14px;
	font-weight: 600;

	&:hover {
		background-color: $secondary-color-light;
		color: $primary-color;
	}
}
</style>