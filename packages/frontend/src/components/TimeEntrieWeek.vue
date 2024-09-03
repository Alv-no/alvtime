<template>
  <div class="container">
    <HourInput
      v-for="timeEntrie in timeEntries"
      :key="timeEntrie.date"
      :time-entrie="timeEntrie"
      :is-locked="timeEntrie.locked"
    />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import HourInput from "./HourInput.vue";
import config from "@/config";
import { Moment } from "moment";
import { FrontendTimentrie } from "@/store/timeEntries";
import { Task } from "@/store/tasks";

export default Vue.extend({
  components: {
    HourInput,
  },
  props: {
    task: {
      type: Object as () => Task,
      default: (): Task => {
        return {} as Task;
      },
    },
    week: { type: Array as () => Moment[], default: () => [] },
  },

  computed: {
    timeEntries(): FrontendTimentrie[] {
      return this.week.map((day: Moment) => {
        const timeEntrie = this.findEntrieInState(day);
        if (!timeEntrie) {
          return this.zeroEntrie(day);
        }
        return timeEntrie;
      });
    },
  },

  methods: {
    findEntrieInState(day: Moment): FrontendTimentrie | undefined {
      const date = day.format(config.DATE_FORMAT);
      const taskId = this.task.id;
      const locked = this.task.locked;
      const timeEntry = this.$store.state.timeEntriesMap[`${date}${taskId}`];
      return (
        timeEntry && {
          id: timeEntry.id,
          value: timeEntry.value,
          comment: timeEntry.comment,
          commentedAt: timeEntry.commentedAt,
          taskId,
          date,
          locked,
        }
      );
    },

    zeroEntrie(day: Moment): FrontendTimentrie {
      return {
        id: 0,
        date: day.format(config.DATE_FORMAT),
        value: "0",
        taskId: this.task.id,
        locked: this.task.locked,
      };
    },
  },
});
</script>

<style scoped>
.container {
  display: grid;
  grid-template-columns: repeat(7, auto);
  gap: 1.51rem;
  align-items: center;
}
</style>
