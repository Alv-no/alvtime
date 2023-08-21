<template>
  <canvas id="context"></canvas>
</template>

<script lang="ts">
import {
  Chart,
  CategoryScale,
  BarController,
  LinearScale,
  BarElement,
  Title,
  Tooltip,
} from "chart.js";

import Vue from "vue";
import ChartDataLabels, { Context } from "chartjs-plugin-datalabels";
import { HorizontalLinePlugin } from "../utils/horizontal-line-chart-plugin";	
import { ChartConfiguration } from "chart.js";

Chart.register(
  BarController,
  CategoryScale,
  LinearScale,
  BarElement,
  Title,
  ChartDataLabels,
  HorizontalLinePlugin,
  Tooltip
);

let chart: Chart | null = null;
const budgetedInvoiceRate = 90;

export default Vue.extend({
  computed: {
    invoiceStatistics: function() {
      return this.$store.getters.getInvoiceStatistics;
    },
  },
  watch: {
    invoiceStatistics: function() {
      if (!chart) return;

      chart.data.labels = this.invoiceStatistics?.labels;
      chart.data.datasets = [
        {
          label: "Faktureringsgrad",
          data: this.invoiceStatistics?.invoiceRate || [],
          backgroundColor: "#041938dd",
          borderWidth: 1,
        },

        {
          label: "Grad interntimer",
          data: this.invoiceStatistics?.nonBillableInvoiceRate || [],
          backgroundColor: "#1c92d0dd",
          borderWidth: 1,
        },
      ];

      chart.update();
    },
  },
  mounted() {
    const chartConfig: ChartConfiguration = {
      type: "bar",
      data: {
        datasets: [],
      },
      options: {
        plugins: {
          horizontalLinePlugin: {
            yValue: budgetedInvoiceRate,
            color: "rgba(240, 50, 50, .6)",
          },
          datalabels: {
            align: "center",
            anchor: "center",
            font: {
              weight: "bold",
            },
            color: "#fff",
            formatter: (value: number, context: Context): string => {
              if (value === 0) {
                return "";
              }

              return `${this.$store.getters.getInvoiceLabel(
                context.datasetIndex,
                context.dataIndex
              )}`;
            },
          },
        },
        maintainAspectRatio: false,
        responsive: true,
        scales: {
          y: {
            stacked: true,
            ticks: {
              callback: (
                value: string | number,
                _index: number,
                _ticks: unknown[]
              ) => {
                return `${value} %`;
              },
            },
          },
          x: {
            stacked: true,
          },
        },
      },
    };
    const context = document.getElementById("context") as HTMLCanvasElement;
    chart = new Chart(context, chartConfig);
  },
  async created() {
    // subscribe to mutations
    this.$store.subscribe(mutation => {
      if (mutation.type === "SET_INVOICE_STATISTIC_FILTERS") {
        this.$store.dispatch("FETCH_INVOICE_STATISTICS");
      }
    });

    // fetch invoice statistics
    await this.$store.dispatch("FETCH_INVOICE_STATISTICS");
  },
});
</script>
<style scoped>
#context {
  height: 100%;
  width: 100%;
}</style
>>
