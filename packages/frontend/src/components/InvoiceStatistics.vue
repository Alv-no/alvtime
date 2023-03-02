<template>
  <div class="wrapper">
    <div class="date-inputs">
      <simple-date-picker :label="'Fra måned'" :defaultDate="initialFromDate"
        @dateSelected="setFromDate($event)"></simple-date-picker>
      <simple-date-picker :label="'Til måned'" :defaultDate="initialToDate"
        @dateSelected="setToDate($event)"></simple-date-picker>
    </div>
    <div class="interval-shortcuts">
      <interval-shortcuts @januarSelected="setInterval($event)" :typeInterval="halfyearInterval"></interval-shortcuts>
    </div>
    <div class="interval-shortcuts">
      <interval-shortcuts @januarSelected="setInterval($event)" :typeInterval="yearInterval"></interval-shortcuts>
    </div>
    <div class="interval-shortcuts">
      <interval-shortcuts @januarSelected="setInterval($event)" :typeInterval="quarterInterval"></interval-shortcuts>
    </div>
    <div class="statistics">
      <div class="statistic-cards">
        <invoice-statistic-card v-for="cardData in summarizedStatistics" :key="cardData.title"
          :cardData="cardData"></invoice-statistic-card>
      </div>
      <div class="chart-wrapper">
        <invoice-chart></invoice-chart>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import InvoiceChart from "./InvoiceChart.vue";
import SimpleDatePicker from "./SimpleDatePicker.vue";
import IntervalShortcuts from "./IntervalShortcuts.vue";
import { InvoiceStatisticsFilters } from "../store/invoiceRate";
import InvoiceStatisticCard from "./InvoiceStatisticCard.vue";
import { fromPairs } from "lodash";
export default Vue.extend({
  components: { InvoiceChart, SimpleDatePicker, InvoiceStatisticCard, IntervalShortcuts },
  data() {
    const filters = this.$store.getters
      .getInvoiceFilters as InvoiceStatisticsFilters;
    return {
      initialFromDate: filters.fromDate,
      initialToDate: filters.toDate,
      summarizedStatistics: [],
      halfyearInterval: "Siste halvår",
      yearInterval: "Siste år",
      quarterInterval: "Siste kvartal",
      unsubscribe: () => { },
    };
  },
  created() {
    this.$store.subscribe(mutation => {
      if (mutation.type === "SET_INVOICE_STATISTIC") {
        this.summarizedStatistics = this.$store.getters.getSummarizedStatistics;
      }
    });
  },

  beforeDestroy() {
    this.unsubscribe();
  },
  methods: {
    setToDate(event: string) {
      this.initialToDate = event;
      this.$store.dispatch("CHANGE_INVOICE_FILTERS", { toDate: event });
    },

    setFromDate(event: string) {
      this.initialFromDate = event;
      this.$store.dispatch("CHANGE_INVOICE_FILTERS", { fromDate: event });
    },
    setDateFilters(toDate: string, fromDate: string) {
      this.$store.dispatch("CHANGE_INVOICE_FILTERS", { toDate, fromDate });
    },
    setInterval(event: { toDate: string, fromDate: string }) {
      this.initialFromDate = event.fromDate;
      this.initialToDate = event.toDate;
      this.setDateFilters(event.toDate, event.fromDate);
    },
  },
});
</script>

<style scoped>
.chart-wrapper {
  width: 100%;
  padding: 1rem;
  min-height: 50vh;
}

.wrapper {
  width: 100%;
  padding: 1rem;
}

.statistics {
  display: flex;
  flex-direction: row;
  gap: 0.25rem;
}

.statistic-cards {
  min-width: 200px;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.statistic-cards>div {
  flex: 1 0;
}

@media only screen and (max-width: 650px) {
  .statistics {
    flex-direction: column;
  }

  .statistic-cards {
    flex-direction: row;
    flex-wrap: wrap;
  }
}

.date-inputs {
  display: flex;
  justify-content: start;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}
</style>
