<template>
	<h2 class="title">
		Timebankhistorikk
	</h2>
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
 