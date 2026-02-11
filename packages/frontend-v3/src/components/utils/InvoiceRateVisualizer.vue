<template>
	<div
		class="invoice-rate-visualizer"
		@mouseenter="hovering = true"
		@mouseleave="hovering = false"
	>
		<div
			id="invoice-rate-expander"
			class="expander"
			:class="{ visible: hovering }"
		>
			<div
				class="previous-months"
			>
				<div
					v-for="period in lastFourMonthsBeforeCurrentPeriod"
					:key="period.formattedMonthAndYear"
					class="previous-month-wrapper"
				>
					<p class="previous-month">
						{{ period?.formattedMonthAndYear ?? "" }}
					</p>
					<p class="previous-invoice-rate">
						{{ `${period?.rate ?? 0}%` }}
					</p>
				</div>
			</div>
		</div>
		<div class="current-invoice-rate">
			<p class="heading">
				Fakturering
			</p>
			<p class="invoice-rate">
				{{ `${currentPeriod?.rate ?? 0}%` }}
			</p>
			<p class="current-period">
				{{ currentPeriod?.formattedMonthAndYear ?? "" }}
			</p>
		</div>
	</div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from "vue";
import { storeToRefs } from "pinia";
import { useTimeEntriesStore } from "@/stores/timeEntriesStore";
import { monthOfYear } from "@/utils/dateHelper";

const timeEntriesStore = useTimeEntriesStore();
const { invoiceRate } = storeToRefs(timeEntriesStore);
const hovering = ref(false);

const toggleExpander = () => {
	const expander = document.getElementById("invoice-rate-expander");
	const isMobile = window.innerWidth < 768;
	
	if (hovering.value && !isMobile) {
		expander?.classList.add("visible");
		expander?.style.setProperty("width", expander?.scrollWidth + "px");
	} else {
		expander?.classList.remove("visible");
		expander?.style.setProperty("width", "0");
	}
};

const currentPeriod = computed(() => {
	const lastPeriod = invoiceRate.value[invoiceRate.value.length - 1];

	return lastPeriod ? {
		...lastPeriod,
		formattedMonthAndYear: `${monthOfYear(lastPeriod?.month)} '${lastPeriod.year.toString().slice(-2)}`,
	} : { rate: 0, month: 1, year: new Date().getFullYear(), formattedMonthAndYear: ""};
});

const lastFourMonthsBeforeCurrentPeriod = computed(() => {
	const currentMonthIndex = Math.max(0, invoiceRate.value.length - 1);
	const startIndex = Math.max(0, currentMonthIndex - 4);
	const filteredMonths = invoiceRate.value.slice(startIndex, currentMonthIndex);

	return filteredMonths.map((item) => ({
		...item,
		formattedMonthAndYear: `${monthOfYear(item.month).slice(0,3)} '${item.year.toString().slice(-2)}`,
	}));
});

watch(hovering, () => {
	toggleExpander();
});
</script>

<style scoped lang="scss">
.invoice-rate-visualizer {
	box-sizing: border-box;
	background-color: $secondary-color-light;
	height: 96px;
	padding: 10px;
	border-radius: 10px;
	display: flex;
	justify-content: space-between;
	align-items: center;
	text-align: center;
	
	.expander {
		display: flex;
		flex-direction: row;
		align-items: center;
		justify-content: flex-start;
		gap: 10px;
		width: 0;
		overflow: hidden;
		transition: width .5s ease;

		.previous-months {
			display: grid;
			grid-template-columns: 1fr 1fr;
			grid-template-rows: 1fr 1fr;
			gap: 12px;
			margin: 24px 12px 24px;
	
			.previous-month-wrapper {
				.previous-month {
					font-size: .7rem;
					font-weight: 500;
					margin: 0 0 2px;
					padding: 0;
				}
	
				.previous-invoice-rate {
					font-size: 1rem;
					font-weight: 700;
					margin: 0;
					padding: 0;
				}
			}
		}
	}

	.current-invoice-rate {
		background-color: $secondary-color-light;
		margin: 12px;
		display: flex;
		flex-direction: column;
		justify-content: space-between;
		align-items: center;
		gap: 4px;

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


}
</style>
