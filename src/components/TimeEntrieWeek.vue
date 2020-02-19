<template>
  <div class="container">
    <TimeEntrieText :task="task" />
    <HourInput
      v-for="timeEntrie in timeEntries"
      :key="timeEntrie.date"
      :timeEntrie="timeEntrie"
    />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import TimeEntrieText from "./TimeEntrieText.vue";
import HourInput from "./HourInput.vue";
import config from "@/config";
import moment from "moment";
import { TimeEntrie } from "@/store";

export default Vue.extend({
  components: {
    TimeEntrieText,
    HourInput,
  },
  props: ["task", "week"],

  computed: {
    timeEntries(): TimeEntrie[] {
      return this.week.map((day: moment.Moment) => {
        const timeEntrie = this.findEntrieInState(day);
        if (!timeEntrie) {
          return this.zeroEntrie(day);
        }
        return timeEntrie;
      });
    },
  },

  methods: {
    findEntrieInState(day: moment.Moment): TimeEntrie {
      return this.$store.state.timeEntries.find(
        (entrie: TimeEntrie) =>
          entrie.date === day.format(config.DATE_FORMAT) &&
          entrie.taskId === this.task.id
      );
    },

    zeroEntrie(day: moment.Moment): TimeEntrie {
      return {
        id: 0,
        date: day.format(config.DATE_FORMAT),
        value: "0",
        taskId: this.task.id,
      };
    },
  },
});
</script>

<style scoped>
.container {
  display: grid;
  grid-template-columns: 1fr auto auto auto auto auto auto auto;
  gap: 1.51rem;
  align-items: center;
  color: #000;
  padding: 0 1rem;
}
</style>
