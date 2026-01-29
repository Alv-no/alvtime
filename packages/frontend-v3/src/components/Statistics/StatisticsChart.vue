<template>
	<div
		ref="chartRef"
		class="chart"
	>
		<svg ref="svgRef" />
		<div
			ref="tooltipRef"
			class="tooltip"
		/>
	</div>
</template>

<script setup lang="ts">
import * as d3 from "d3";
import type { Series, SeriesPoint } from "d3";
import type { Statistics } from "@/stores/statisticsStore.ts";
import { computed, onBeforeUnmount, onMounted, ref, watch } from "vue";

const props = defineProps<{
  statistics: Statistics,
  period: 0 | 1 | 2 | 3
}>();

type StackedData = Series<Record<string, number | string>, string>;
type StackedPoint = SeriesPoint<Record<string, number | string>>;

const svgRef = ref<SVGSVGElement | null>(null);
const chartRef = ref<HTMLDivElement | null>(null);
const tooltipRef = ref<HTMLDivElement | null>(null);

const getDateFormatter = (period: 0 | 1 | 2 | 3) => {
	switch (period) {
	case 0: // Day
		return new Intl.DateTimeFormat("nb-NO", {
			day: "numeric",
			month: "short",
		});
	case 1: // Week
		return new Intl.DateTimeFormat("nb-NO", {
			day: "numeric",
			month: "short",
		});
	case 2: // Month
		return new Intl.DateTimeFormat("nb-NO", {
			month: "long",
			year: "numeric",
		});
	case 3: // Year
		return new Intl.DateTimeFormat("nb-NO", {
			year: "numeric",
		});
	}
};

const formatLabel = (dateStr: string, period: 0 | 1 | 2 | 3) => {
	const date = new Date(dateStr);
	const formatter = getDateFormatter(period);

	if (period === 1) {
		// For weeks, show the week start date
		return `Uke ${getWeekNumber(date)}`;
	}

	return capitalize(formatter.format(date));
};

const getWeekNumber = (date: Date) => {
	const d = new Date(Date.UTC(date.getFullYear(), date.getMonth(), date.getDate()));
	const dayNum = d.getUTCDay() || 7;
	d.setUTCDate(d.getUTCDate() + 4 - dayNum);
	const yearStart = new Date(Date.UTC(d.getUTCFullYear(), 0, 1));
	return Math.ceil((((d.getTime() - yearStart.getTime()) / 86400000) + 1) / 7);
};

const capitalize = (value: string) =>
	value ? value.charAt(0).toUpperCase() + value.slice(1) : value;

const data = computed(() =>
	props.statistics.start.map((start, i) => ({
		label: formatLabel(start.toString(), props.period),
		billableHours: props.statistics.billableHours[i] ?? 0,
		nonBillableHours: props.statistics.nonBillableHours[i] ?? 0,
		invoiceRate: (props.statistics.invoiceRate[i] ?? 0) * 100,
		nonBillableInvoiceRate: (props.statistics.nonBillableInvoiceRate[i] ?? 0) * 100
	}))
);

//TODO: Add tooltip on hover
const draw = () => {
	if (!svgRef.value || !chartRef.value) return;

	const rect = chartRef.value.getBoundingClientRect();
	const margin = { top: 16, right: 16, bottom: 32, left: 48 };
	const width = rect.width || 560;
	const height = rect.height || 280;

	const svg = d3
		.select(svgRef.value)
		.attr("width", width)
		.attr("height", height);

	svg.selectAll("*").remove();

	const tooltip = d3.select(tooltipRef.value);

	const x = d3
		.scaleBand(data.value.map((d) => d.label), [margin.left, width - margin.right])
		.padding(0.2);

	const maxRateWorked = Math.max(...data.value.map((d) => d.invoiceRate + d.nonBillableInvoiceRate));
	const yMax = maxRateWorked * 1.1;

	const y = d3
		.scaleLinear([0, yMax], [height - margin.bottom, margin.top]);

	const scaledData = data.value.map((d) => ({
		...d,
		billableHours: d.invoiceRate,
		nonBillableHours: d.nonBillableInvoiceRate,
	}));

	const stack = d3.stack().keys(["billableHours", "nonBillableHours"]);
	const series = stack(scaledData as Array<Record<string, number | string>>);

	const colors = d3
  	.scaleOrdinal<string>(["billableHours", "nonBillableHours"])
  	.range(["#9dac86", "#e6c08d"]);

	svg
  	.append("g")
  	.selectAll("g")
  	.data(series)
  	.join("g")
  	.attr("fill", (d: StackedData) => colors(d.key))
  	.selectAll("rect")
  	.data((d: StackedData) => d)
  	.join("rect")
  	.attr("x", (d: StackedPoint) => x((d.data as any).label)!)
  	.attr("y", (d: StackedPoint) => y(d[1]))
  	.attr("height", (d: StackedPoint) => y(d[0]) - y(d[1]))
  	.attr("width", x.bandwidth())
		.on("mouseenter", (event: MouseEvent, d: StackedPoint) => {
			const dataPoint = d.data as any;
			const containerRect = chartRef.value!.getBoundingClientRect();
			tooltip
				.style("opacity", 1)
				.style("left", `${event.clientX - containerRect.left + 120}px`)
				.style("top", `${event.clientY - containerRect.top + 40}px`)
				.html(`
  				<strong>${dataPoint.label}</strong><br>
  				<strong>Faktureringsgrad:</strong> ${dataPoint.invoiceRate.toFixed(1)}%
  			`);
		})
		.on("mousemove", (event: MouseEvent) => {
			const containerRect = chartRef.value!.getBoundingClientRect();
			tooltip
				.style("left", `${event.clientX - containerRect.left + 120}px`)
				.style("top", `${event.clientY - containerRect.top + 40}px`);
		})
		.on("mouseleave", () => {
			tooltip.style("opacity", 0);
		});

	series.forEach((s: StackedData) => {
		const key = s.key as "billableHours" | "nonBillableHours";
		svg
			.append("g")
			.selectAll("text")
			.data(s)
			.join("text")
			.attr("x", (d: StackedPoint) => x((d.data as any).label)! + x.bandwidth() / 2)
			.attr("y", (d: StackedPoint) => {
				const barHeight = y(d[0]) - y(d[1]);
				return y(d[1]) + barHeight / 2;
			})
			.attr("text-anchor", "middle")
			.attr("dominant-baseline", "middle")
			.attr("fill", "#333")
			.attr("font-size", "12px")
			.attr("font-weight", "bold")
			.text((_d: StackedPoint, i: number) => {
				const originalValue = data.value[i][key];
				return originalValue > 0 ? originalValue.toFixed(1) : "";
			});
	});

	svg
  	.append("g")
  	.attr("transform", `translate(0,${height - margin.bottom})`)
  	.call(d3.axisBottom(x))
  	.selectAll("text")
  	.style("font-size", "14px")
  	.style("font-weight", "bold");

	svg
  	.append("g")
  	.attr("transform", `translate(${margin.left},0)`)
		.call(d3.axisLeft(y).tickFormat((d: number) => `${d} %`))
  	.selectAll("text")
  	.style("font-size", "14px")
  	.style("font-weight", "bold");
};

onMounted(() => {
	draw();
});
watch(data, draw, { deep: true });

onBeforeUnmount(() => {
	d3.select(svgRef.value).selectAll("*").remove();
});

</script>

<style scoped lang="scss">
.chart {
  display: flex;
  justify-content: center;
  align-items: center;
  width: 100%;
  height: 100%;
}

svg {
  width: 100%;
  height: 100%;
}

.tooltip {
  position: absolute;
  background: rgba(0, 0, 0, 0.8);
  color: white;
  padding: 8px 12px;
  border-radius: 4px;
  font-size: 12px;
  pointer-events: none;
  opacity: 0;
  transition: opacity 0.2s ease;
  white-space: nowrap;
  z-index: 10;
}
</style>