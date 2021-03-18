<template>
  <CenterColumnWrapper>
    <h1>Halla, dashboard her</h1>
    <div class="chart-container">
      <Chart class="chart" :data="monthSums"></Chart>
      <Chart class="chart"></Chart>
      <Chart class="chart"></Chart>
    </div>
  </CenterColumnWrapper>
</template>

<script lang="ts">
import Vue from "vue";
import Chart from "@/components/CommitChart.vue";
import CenterColumnWrapper from "@/components/CenterColumnWrapper.vue";
import moment from "moment";

export default Vue.extend({
  name: "Home",
  components: {
    Chart,
    CenterColumnWrapper,
  },
  beforeCreate() {
    this.$store.dispatch("FETCH_WEEK_ENTRIES");
  },
  computed: {
    monthSums() {
      const timeEntries = this.$store.state.timeEntries;
      if (!timeEntries) return;

      const monthSums: number[] = Array(12).fill(0);
      const thisYear = moment().year();

      for (const timeEntry of timeEntries) {
        const timeEntryYear = moment(timeEntry.date).year();
        const month = moment(timeEntry.date).month();
        const value: number = timeEntry.value;
        if (timeEntryYear === thisYear) {
          monthSums[month] += Number(value);
        }
      }
      return monthSums;
    },
  },
});
</script>

<style scoped>
.chart-container {
  display: flex;
}

.chart {
  width: 30%;
  margin: 0 10px;
}
</style>
