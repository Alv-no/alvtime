<template>
  <div class="grid">
    <TimeEntrieText :timeEntrie="timeEntrie" />
    <HourInput :value="timeEntrie.value" />
    <HourInput :value="timeEntrie.value" />
    <HourInput :value="timeEntrie.value" />
    <HourInput :value="timeEntrie.value" />
    <HourInput :value="timeEntrie.value" />
    <HourInput :value="timeEntrie.value" />
    <HourInput :value="timeEntrie.value" />
  </div>
</template>

<script>
import TimeEntrieText from "./TimeEntrieText";
import HourInput from "./HourInput";
export default {
  components: {
    TimeEntrieText,
    HourInput,
  },
  props: ["timeEntrie"],
};
</script>

<style scoped>
.grid {
  display: grid;
  grid-template-columns: 1fr auto auto auto auto auto auto auto;
  gap: 1rem;
  align-items: center;
  color: #000;
  padding: 0 1rem;
}
</style>
