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
import { InvoiceStatistics } from "../store/invoiceRate";
import { HorizontalLinePlugin } from "../utils/horizontal-line-chart-plugin";

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

export default Vue.extend({
  data: (): {
    invoiceStatistics: InvoiceStatistics | null;
    chart: Chart | null;
    unsubscribe: () => void;
  } => {
    return {
      invoiceStatistics: null,
      chart: null,
      unsubscribe: () => {},
    };
  },
  mounted() {
    const context = document.getElementById("context") as HTMLCanvasElement;
    this.chart = new Chart(context, {
      type: "bar",
      data: {
        datasets: [],
      },
      options: {
        plugins: {
          horizontalLinePlugin: {
            yValue: 90,
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
          },

          x: {
            stacked: true,
          },
        },
      },
    });
  },
  async created() {
    this.$store.subscribe(mutation => {
      if (mutation.type === "SET_INVOICE_STATISTIC_FILTERS") {
        this.$store.dispatch("FETCH_INVOICE_STATISTICS").then(() => {
          this.updateChartData();
        });
      }
    });
    await this.$store.dispatch("FETCH_INVOICE_STATISTICS");
    this.updateChartData();
  },
  beforeDestroy() {
    this.unsubscribe();
  },
  methods: {
    updateChartData() {
      this.invoiceStatistics = this.$store.getters.getInvoiceStatistics;

      if (!this.chart) {
        return;
      }
      this.chart.data.labels = this.invoiceStatistics?.labels;

      this.chart.data.datasets = [
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

      this.chart.update();
    },
  },
});
</script>
<style scoped>
 #context {
   height: 100%;
   width: 100%;
 }
</style>>
