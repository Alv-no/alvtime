<template>
	<div class="invoice-rate-visualizer">
		<p class="heading">
			Fakturering
		</p>
		<p class="invoice-rate">
			{{ `${invoiceRate}%` }}
		</p>
		<p class="current-period">
			{{ currentPeriod }}
		</p>
	</div>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { storeToRefs } from "pinia";
import { useTimeEntriesStore } from "@/stores/timeEntriesStore";

const timeEntriesStore = useTimeEntriesStore();
const { invoiceRate } = storeToRefs(timeEntriesStore);

const currentPeriod = computed(() => {
	const now = new Date();
	const month = now.toLocaleString("default", { month: "short" });
	const year = now.getFullYear() - 2000;
	return `${month} '${year}`;
});
</script>

<style scoped lang="scss">
.invoice-rate-visualizer {
	box-sizing: border-box;
	background-color: $secondary-color-light;
	height: 96px;
	width: 128px;
	padding: 10px;
	border-radius: 10px;
	display: flex;
	flex-direction: column;
	justify-content: space-between;
	align-items: center;
	text-align: center;

	p {
		position: relative;
		top: 2px;
	}

	p.heading {
		font-size: .9rem;
		padding: 0;
		margin: 0;
	}

	p.invoice-rate {
		top: 4px;
		font-size: 2.4rem;
		font-weight: 700;
		margin: 0;
		padding: 0;
	}

	p.current-period {
		font-size: .8rem;
		margin: 0;
		padding: 0;
	}
}
</style>
