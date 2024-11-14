<template>
  <div class="wrapper">
    <div class="chart-controls-wrapper">
      <div class="date-inputs-wrapper">
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
        <div>
          <b>Oppløsning</b><br />
          <div class="granularity-select-wrapper">
            <select class="granularity-select" @change="setGranularity">
              <option
                v-for="(option, index) in granularityOptions"
                :key="index"
                :value="option.value"
                :selected="option.value == selectedGranularity"
                >{{ option.label }}</option
              >
            </select>
          </div>
        </div>
      </div>
      <div class="interval-shortcuts">
        <md-button
          v-for="(preset, index) in invoiceStatisticPresets"
          :key="index"
          class="md-primary md-raised"
          @click="setDateFiltersFromPreset(preset.type, preset.granularity)"
          >{{ preset.label }}</md-button
        >
      </div>
    </div>
    <div class="statistics">
      <div class="statistic-cards">
        <invoice-statistic-card
          v-for="cardData in summarizedStatistics"
          :key="cardData.title"
          :card-data="cardData"
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
import {
  InvoiceStatisticsFilters,
  InvoiceStatisticsPreset,
  InvoiceStatisticsPresetTypes,
  InvoicePeriods,
} from "../store/invoiceRate";
import InvoiceStatisticCard from "./InvoiceStatisticCard.vue";
import { createTimeString } from "@/utils/timestamp-text-util";

const invoiceStatisticPresets: InvoiceStatisticsPreset[] = [
  {
    type: InvoiceStatisticsPresetTypes.YEAR_INTERVAL,
    label: "Siste år",
    granularity: InvoicePeriods.Monthly,
  },
  {
    type: InvoiceStatisticsPresetTypes.HALF_YEAR_INTERVAL,
    label: "Siste halvår",
    granularity: InvoicePeriods.Monthly,
  },
  {
    type: InvoiceStatisticsPresetTypes.QUARTER_INTERVAL,
    label: "Siste kvartal",
    granularity: InvoicePeriods.Weekly,
  },
  {
    type: InvoiceStatisticsPresetTypes.WEEK_INTERVAL,
    label: "Siste uke",
    granularity: InvoicePeriods.Daily,
  },
];

const granularityOptions = [
  {
    value: InvoicePeriods.Annualy,
    label: "År",
  },
  {
    value: InvoicePeriods.Monthly,
    label: "Måned",
  },
  {
    value: InvoicePeriods.Weekly,
    label: "Uke",
  },
  {
    value: InvoicePeriods.Daily,
    label: "Dag",
  },
];

export default Vue.extend({
  components: { InvoiceChart, SimpleDatePicker, InvoiceStatisticCard },
  data() {
    const filters = this.$store.getters
      .getInvoiceFilters as InvoiceStatisticsFilters;
    return {
      initialFromDate: filters.fromDate,
      initialToDate: filters.toDate,
      invoiceStatisticPresets,
      granularityOptions,
      selectedGranularity: filters.granularity,
      summarizedStatistics: [],
      unsubscribe: () => {},
    };
  },
  created() {
    this.$store.subscribe(mutation => {
      if (mutation.type === "SET_INVOICE_STATISTIC") {
        this.summarizedStatistics = this.$store.getters.getSummarizedStatistics;
      } else if (mutation.type === "SET_INVOICE_STATISTIC_FILTERS") {
        const {
          granularity,
          fromDate,
          toDate,
        } = this.$store.getters.getInvoiceFilters;
        this.selectedGranularity = granularity;
        this.initialFromDate = fromDate;
        this.initialToDate = toDate;
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
    setInterval(event: {
      fromDate: string;
      toDate: string;
      granularity?: InvoicePeriods;
    }) {
      const { fromDate, toDate, granularity } = event;
      this.$store.dispatch("CHANGE_INVOICE_FILTERS", {
        fromDate,
        toDate,
        granularity,
      });
    },
    setGranularity(event: any) {
      const granularity: InvoicePeriods = +event.target.value;
      const currentFilters: InvoiceStatisticsFilters = this.$store.getters
        .getInvoiceFilters;
      this.$store.dispatch("CHANGE_INVOICE_FILTERS", {
        ...currentFilters,
        granularity,
      });
    },
    setDateFiltersFromPreset(
      type: InvoiceStatisticsPresetTypes,
      granularity: InvoicePeriods
    ) {
      const toDate = new Date();
      let fromDate = new Date(toDate.getTime());

      const toMonth = toDate.getMonth() + 1; // 0-indexed, so have to add one
      const toYear = toDate.getFullYear();
      switch (type) {
        case InvoiceStatisticsPresetTypes.YEAR_INTERVAL:
          fromDate.setMonth(toMonth);
          fromDate.setFullYear(toYear - 1);
          break;
        case InvoiceStatisticsPresetTypes.HALF_YEAR_INTERVAL:
          fromDate.setMonth(toMonth - 6);
          break;
        case InvoiceStatisticsPresetTypes.QUARTER_INTERVAL:
          fromDate.setMonth(toMonth - 3);
          break;
        case InvoiceStatisticsPresetTypes.WEEK_INTERVAL: {
          const toDay = toDate.getDate();
          fromDate.setMonth(toMonth);
          fromDate.setDate(toDay - 7);
          break;
        }
        default:
          console.warn(`Unknown preset type: ${type}`);
          return;
      }

      const formattedFromDate = createTimeString(
        fromDate.getFullYear(),
        fromDate.getMonth(),
        fromDate.getDate()
      );
      const formattedToDate = createTimeString(
        toYear,
        toMonth,
        toDate.getDate()
      );

      this.setInterval({
        fromDate: formattedFromDate,
        toDate: formattedToDate,
        granularity,
      });
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

.interval-shortcuts {
  display: flex;
  justify-content: flex-end;
  gap: 0.1rem;
  margin: 1rem 0;
}
.filter-wrapper {
  justify-content: space-between;
  display: flex;
  align-items: center;
}
.md-button {
  max-width: 110px;
  --md-theme-default-primary: #1c92d0;
}
.granularity-select {
  padding: 0.6rem;
  min-width: 90px;
  appearance: none;
}

.granularity-select-wrapper {
  position: relative;
}

.granularity-select-wrapper::after {
  content: "▼";
  font-size: 0.6rem;
  top: 10px;
  right: 14px;
  position: absolute;
  pointer-events: none;
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

.chart-controls-wrapper {
  display: flex;
  justify-content: space-between;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}

.date-inputs-wrapper {
  display: flex;
  flex-direction: row;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}
</style>
