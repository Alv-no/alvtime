<template>
	<div
		class="strip-container"
	>
		<div
			class="expand-container"
			:class="{ 'active': filteredEntries.length > 1 }"
			@click="toggleShowEntries"
		>
			<div
				class="expand-button-container"
			>
				<button v-if="filteredEntries.length > 1">
					<FeatherIcon
						:name="showEntries ? 'chevron-up' : 'chevron-down'"
						:size="24"
					/>
				</button>
				<p class="first">
					{{ timeBankTransaction.date }}
				</p>
			</div>
			<p>{{ transactionType }}</p>
			<p>{{ `${totalHoursInTransaction} timer` }}</p>
			<p>{{ compensationRate }}</p>
		</div>
		<div class="value-and-button">
			<p class="last">
				{{ actualValue }}
			</p>
			<button
				v-if="isActive"
				@click="cancelTimeBankPayout(timeBankTransaction.date)"
			>
				<FeatherIcon
					class="icon"
					name="trash"
					:size="24"
				/>
			</button>
		</div>
	</div>
	<div
		v-if="compensationRate === '' && filteredEntries.length > 1"
		class="entry-expandable"
		:class="{ 'visible': showEntries }"
	>
		<div class="entry-expandable-content">
			<div
				v-for="transaction in filteredEntries"
				:key="`${transaction.date}-${transaction.compensationRate}-${transaction.hours}`"
				class="strip-container"
			>
				<div class="expand-container">
					<div class="expand-button-container" />
					<p />
					<p>{{ `${transaction.hours} timer` }}</p>
					<p>{{ `${transaction.compensationRate * 100}%` }}</p>
				</div>
				<div>
					<p class="last">
						{{ `${transaction.hours * (timeBankTransaction.type !== 2 ? transaction.compensationRate : 1)}` }}
					</p>
				</div>
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import { defineProps, computed, ref } from "vue";
import { type TimeBankEntry } from "@/stores/timeBankStore";
import FeatherIcon from "../utils/FeatherIcon.vue";
import { useTimeBankStore } from "@/stores/timeBankStore";

const showEntries = ref(false);
const timeBankStore = useTimeBankStore();
const { cancelTimeBankPayout } = timeBankStore;

const { timeBankTransaction } = defineProps<{
	timeBankTransaction: {
		date: string;
		type: number;
		entries: Array<TimeBankEntry>;
	};
}>();

const totalHoursInTransaction = computed(() => {
	return timeBankTransaction.entries.reduce((total, entry) => total + entry.hours, 0);
});

const transactionType = computed(() => {
	switch (timeBankTransaction.type) {
	case 0:
		return "Opptjent";
	case 1:
		return "Utbetaling";
	case 2:
		return "Flex";
	default:
		return "Ukjent type";
	}
});

const compensationRate = computed(() => {
	const rates = timeBankTransaction.entries.map(entry => entry.compensationRate);
	const allSame = rates.every(rate => rate === rates[0]);
	return allSame ? `${rates[0] * 100}%` : "";
});

const actualValue = computed(() => {
	if(timeBankTransaction.type === 2)
		return totalHoursInTransaction.value;

	const total = timeBankTransaction.entries.reduce((sum, entry) => sum + entry.hours * entry.compensationRate, 0);
	return total;
});

const isActive = computed(() => {
	return timeBankTransaction.entries.every(entry => entry.active);
});

const filteredEntries = computed(() => {
	return timeBankTransaction.entries.filter(entry => entry.hours !== 0);
});

const toggleShowEntries = () => {
	showEntries.value = !showEntries.value;
};
</script>

<style scoped lang="scss">
.expand-container {
	display: flex;
	justify-content: space-between;
	align-items: center;
	width: 100%;

	&.active {
		cursor: pointer;
	}

	p {
		margin: 0;
		width: 100px;
		text-align: center;
	}

	p.first {
		text-align: left;
		width: 150px;
	}

}
.entry-expandable {
	display: grid;
	grid-template-rows: 0fr;
	transition: grid-template-rows 200ms ease-in-out;

	&.visible {
		grid-template-rows: 1fr;
		border-top: 1px solid rgba(206, 214, 194, 0.5);
	}

	.entry-expandable-content {
		display: flex;
		flex-direction: column;
		overflow: hidden;
	}
}
.strip-container {
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 12px;
	height: 32px;

	.expand-button-container {
		display: flex;
		justify-content: flex-start;
		align-items: center;
		width: 150px;

		button {
			background: none;
			border: none;
			cursor: pointer;
			color: $primary-color;
			position: relative;
			top: -4px;

			&.icon {
				color: $primary-color;
			}
		}
	}

	p {
		margin: 0;
		width: 100px;
		text-align: center;
	}

	p.first, p.last {
		text-align: left;
	}


	.value-and-button {
		width: 100px;
		display: flex;
		justify-content: flex-start;
		align-items: center;

		button {
			background: none;
			border: none;
			cursor: pointer;
			margin-left: 8px;
			color: $primary-color;
			position: relative;
			top: -4px;

			&.icon {
				color: $primary-color;
			}
		}
	}
}
</style>
