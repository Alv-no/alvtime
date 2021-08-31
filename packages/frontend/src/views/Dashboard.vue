<template>
  <CenterColumnWrapper>
    <h1>Her kan du se oversikt over dine registrerte timer for {{ year }}</h1>
    <div class="chart-container">
      <Chart class="chart" :data="monthSums"></Chart>
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
  data() {
    return {
      year: moment().year(),
    };
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
  beforeCreate() {
    this.$store.dispatch("FETCH_WEEK_ENTRIES");
  },
});
</script>

<style scoped>
.chart-container {
  display: flex;
}

.chart {
  width: 50%;
  margin: 0 10px;
}
</style>
