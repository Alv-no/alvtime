<template>
  <div class="wrapper">
    <div class="date-inputs">
      <simple-date-picker
        :label="'Fra måned'"
        :default-date="initialFromDate"
        @dateSelected="setFromDate($event)"
      ></simple-date-picker>
      <simple-date-picker
        :label="'Til måned'"
        :default-date="initialToDate"
        @dateSelected="setToDate($event)"
      ></simple-date-picker>
    </div>
    <div class="statistics">
      <div class="statistic-cards">
        <invoice-statistic-card
          v-for="cardData in summarizedStatistics"
          :key="cardData.title"
          :cardData="cardData"
        ></invoice-statistic-card>
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
import { InvoiceStatisticsFilters } from "../store/invoiceRate";
import InvoiceStatisticCard from "./InvoiceStatisticCard.vue";
export default Vue.extend({
  components: { InvoiceChart, SimpleDatePicker, InvoiceStatisticCard },
  data() {
    const filters = this.$store.getters
      .getInvoiceFilters as InvoiceStatisticsFilters;
    return {
      initialFromDate: filters.fromDate,
      initialToDate: filters.toDate,
      summarizedStatistics: [],
      unsubscribe: () => {},
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
      this.$store.dispatch("CHANGE_INVOICE_FILTERS", { toDate: event });
    },

    setFromDate(event: string) {
      this.$store.dispatch("CHANGE_INVOICE_FILTERS", { fromDate: event });
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

.statistic-cards > div {
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
