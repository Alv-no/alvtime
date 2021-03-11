<template>
  <CenterColumnWrapper>
    <h1>Halla, dashboard her</h1>
    <div class="chart-container">
      <Chart class="chart"></Chart>
      <Chart class="chart"></Chart>
      <Chart class="chart"></Chart>
    </div>
  </CenterColumnWrapper>
</template>

<script lang="ts">
import Vue from "vue";
import Chart from "@/components/CommitChart.vue";
import CenterColumnWrapper from "@/components/CenterColumnWrapper.vue";

export default Vue.extend({
  name: "Home",
  components: {
    Chart,
    CenterColumnWrapper,
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
