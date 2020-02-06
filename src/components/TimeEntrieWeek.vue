<template>
  <div>
    <TimeEntrieText :task="task" />
    <HourInput
      v-for="timeEntrie in timeEntries"
      :key="timeEntrie.date"
      :timeEntrie="timeEntrie"
    />
  </div>
</template>

<script>
import TimeEntrieText from "./TimeEntrieText";
import HourInput from "./HourInput";
import config from "@/config";

export default {
  components: {
    TimeEntrieText,
    HourInput,
  },
  props: ["task", "week"],

  computed: {
    timeEntries() {
      return this.week.map(day => {
        const timeEntrie = this.$store.state.timeEntries.find(
          entrie =>
            entrie.date === day.format(config.DATE_FORMAT) &&
            entrie.taskId === this.task.id
        );
        return timeEntrie
          ? timeEntrie
          : {
              id: 0,
              date: day.format(config.DATE_FORMAT),
              value: 0,
              taskId: this.task.id,
            };
      });
    },
  },
};
</script>

<style scoped>
div {
  display: grid;
  grid-template-columns: 1fr auto auto auto auto auto auto auto;
  gap: 1.51rem;
  align-items: center;
  color: #000;
  padding: 0 1rem;
}
</style>
