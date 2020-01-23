<template>
  <div>
    <div
      v-for="row in rows"
      v-bind:key="row.task.id + row.timeEntrie.value"
      class="grid"
    >
      <TimeEntrieText
        :activity="row.task.name"
        :customer="row.task.customerName"
      />
      <HourInput :timeEntrie="row.timeEntrie" />
    </div>
  </div>
</template>

<script>
import TimeEntrieText from "./TimeEntrieText";
import HourInput from "./HourInput";
import moment from "moment";
import config from "@/config";

export default {
  components: {
    TimeEntrieText,
    HourInput,
  },
  props: ["date"],

  computed: {
    rows() {
      return [...this.rowsWithHours, ...this.rowsWithoutHours].sort(
        (a, b) => a.task.customerName > b.task.customerName
      );
    },

    rowsWithHours() {
      return this.daysTimeEntries.map(entrie => {
        const task = this.$store.getters.getTask(entrie.taskId);
        return this.createRow(task, entrie);
      });
    },

    rowsWithoutHours() {
      return this.$store.getters.favoriteTasks
        .filter(task => !this.isTaskInEntries(task))
        .map(task => this.createRow(task));
    },

    daysTimeEntries() {
      return this.$store.state.timeEntries.filter(entrie =>
        this.isThisDate(entrie.date)
      );
    },
  },

  methods: {
    isTaskInEntries(task) {
      return this.daysTimeEntries.some(entrie => entrie.taskId === task.id);
    },

    isThisDate(date) {
      return date === this.date.format(config.DATE_FORMAT);
    },

    createRow(task, timeEntrie) {
      if (!timeEntrie) {
        timeEntrie = {
          id: 0,
          date: this.date.format(config.DATE_FORMAT),
          value: 0,
          taskId: task.id,
        };
      }

      return { task, timeEntrie };
    },
  },
};
</script>

<style scoped>
.grid {
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
  color: #000;
  padding: 0 1rem;
}
</style>
