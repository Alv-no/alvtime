<template>
	<h2 class="title">
		Timebankhistorikk
	</h2>
	<div class="time-bank-history-legend">
		<div class="legend-item first">
			<span>Dato</span>
		</div>
		<div class="legend-item">
			<span>Type</span>
		</div>
		<div class="legend-item">
			<span>Timer</span>
		</div>
		<div class="legend-item">
			<span>Kompensasjonsrate</span>
		</div>
		<div class="legend-item">
			<span>Faktisk verdi</span>
		</div>
	</div>
	<div
		v-if="timeBankHistory.length > 0"
		class="history-container"
	>
		<div
			v-for="mainEntry in timeBankHistory"
			:key="`${mainEntry.date}-${mainEntry.type}`"
			class="history-entry"
		>
			<TimeBankHistoryStrip :timeBankTransaction="mainEntry" />
		</div>
	</div>
	<div v-else>
		<p>Ingen transaksjoner funnet.</p>
	</div>
</template>

<script setup lang="ts">
import { storeToRefs } from "pinia";
import { useTimeBankStore } from "@/stores/timeBankStore";
import TimeBankHistoryStrip from "./TimeBankHistoryStrip.vue";

const timeBankStore = useTimeBankStore();
const { timeBankHistory } = storeToRefs(timeBankStore);
</script>

<style lang="scss" scoped>
.title {
	margin-top: 2rem;
}

.time-bank-history-legend {
	display: flex;
	background-color: $background-color;
	font-weight: 700;
	text-align: center;
	justify-content: space-between;

	.legend-item {
		padding: 1rem;
		width: 100px;

		&.first {
			width: 150px;
			text-align: left;
		}
	}
}

.history-container {
	display: flex;
	flex-direction: column;
	border-radius: 15px;
	overflow: hidden;
	border: 1px solid rgba(206, 214, 194, 0.5);
}

.history-entry {
	border: 1px solid #eee;
	background-color: $background-color;

	&:nth-child(odd) {
		background-color: lighten($background-color, 2%);
	}

	&:hover {
		background: darken($background-color, 2%);
	}
}

p {
	margin: 2rem;
	text-align: center;
}
</style>
 