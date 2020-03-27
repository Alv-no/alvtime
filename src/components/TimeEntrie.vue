<template>
  <div class="grid">
    <TimeEntrieText :timeEntrie="timeEntrie" />
    <HourInput :value="timeEntrie.value" />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import TimeEntrieText from "./TimeEntrieText.vue";
import HourInput from "./HourInput.vue";

export default Vue.extend({
  components: {
    TimeEntrieText,
    HourInput,
  },
  props: ["timeEntrie"],
});
</script>

<style scoped>
.grid {
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
  color: #000;
  padding: 0 1rem;
}

.textfield {
  width: 3.5rem;
}

.input {
  appearance: none;
  -moz-appearance: textfield;
  width: 1.5rem;
  height: 1rem;
  padding: 5px;
}

.text {
  line-height: 0.3rem;
  text-align: left;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  margin-right: 1rem;
}

.customer_text {
  font-size: 0.8rem;
}

.activity_name_text {
  font-size: 1rem;
}
</style>
