<template>
  <div class="container">
    <HourInput
      v-for="timeEntrie in timeEntries"
      :key="timeEntrie.date"
      :timeEntrie="timeEntrie"
    />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import HourInput from "./HourInput.vue";
import config from "@/config";
import { Moment } from "moment";
import { FrontendTimentrie } from "@/store/timeEntries";

export default Vue.extend({
  components: {
    HourInput,
  },
  props: ["task", "week"],

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
      const task = this.$store.state.timeEntriesMap[`${date}${taskId}`];
      return task && { id: task.id, value: task.value, taskId, date };
    },

    zeroEntrie(day: Moment): FrontendTimentrie {
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
  grid-template-columns: auto auto auto auto auto auto auto;
  gap: 1.51rem;
  align-items: center;
  color: #000;
}
</style>
