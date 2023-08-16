<template>
  <div class="card">
    <h3 class="card-title">{{ cardData.title }}</h3>
    <div class="section" v-for="value in cardData.values" :key="value.title">
      <h4>{{ value.title }}</h4>
      <div class="content">
        <div class="value"><b>{{ value.value }}</b></div>
        <div class="unit">{{ value.unit }}</div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { SummarizedStatistics } from "../store/invoiceRate";
export default Vue.extend({
  props: {
    cardData: {
      type: Object as () => SummarizedStatistics,
      default: {
        title: "",
        values: [],
      },
    },
  },
});
</script>

<style scoped>
.card {
  background-color: #f7f7f7;
  padding: 0.25rem 1rem 1.25rem 1rem;
  display: flex;
  flex-direction: column;
  justify-content: center;
}

.card-title {
  color: #1c92d0;
  font-weight: lighter;
  text-align: center;
}

.section {
  width: 100%;
}

.section div {
  color: #041938;
}

.content {
  display: flex;
  gap: 0.25rem;
  justify-content: center;
}

.value {
  font-size: 35px;
}

.unit {
  font-size: 25px;
}

.value > * {
  margin: 0;
}
</style>
