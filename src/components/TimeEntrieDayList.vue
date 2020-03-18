<template>
  <div>
    <div v-for="row in rows" :key="row.task.id" class="grid">
      <TimeEntrieText :task="row.task" />
      <HourInput :timeEntrie="row.timeEntrie" />
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import TimeEntrieText from "./TimeEntrieText.vue";
import HourInput from "./HourInput.vue";
import moment from "moment";
import config from "@/config";
import { Task, FrontendTimentrie } from "@/store";

interface Row {
  task: Task;
  timeEntrie: FrontendTimentrie;
}

export default Vue.extend({
  components: {
    TimeEntrieText,
    HourInput,
  },
  props: ["date"],

  computed: {
    rows() {
      // @ts-ignore
      return [...this.rowsWithHours, ...this.rowsWithoutHours].sort(sortList);
    },

    rowsWithHours(): Row[] {
      // @ts-ignore
      return this.daysTimeEntries.map((entrie: FrontendTimentrie) => {
        const task = this.$store.getters.getTask(entrie.taskId);
        // @ts-ignore
        return this.createRow(task, entrie);
      });
    },

    rowsWithoutHours() {
      return (
        this.$store.getters.favoriteTasks
          // @ts-ignore
          .filter((task: Task) => !this.isTaskInEntries(task))
          // @ts-ignore
          .map((task: Task) => this.createRow(task))
      );
    },

    daysTimeEntries(): FrontendTimentrie[] {
      return this.$store.state.timeEntries.filter((entrie: FrontendTimentrie) =>
        // @ts-ignore
        this.isThisDate(entrie.date)
      );
    },
  },

  methods: {
    isTaskInEntries(task: Task): boolean {
      // @ts-ignore
      return this.daysTimeEntries.some(
        (entrie: FrontendTimentrie) => entrie.taskId === task.id
      );
    },

    isThisDate(date: string): boolean {
      return date === this.date.format(config.DATE_FORMAT);
    },

    createRow(task: Task, timeEntrie: FrontendTimentrie): Row {
      if (!timeEntrie) {
        timeEntrie = {
          id: 0,
          date: this.date.format(config.DATE_FORMAT),
          value: "0",
          taskId: task.id,
        };
      }

      return { task, timeEntrie };
    },
  },
});

function sortList(a: Row, b: Row) {
  const A = a.task.project.customer.name;
  const B = b.task.project.customer.name;
  if (A > B) {
    return 1;
  } else if (A < B) {
    return -1;
  }

  if (a.task.name > b.task.name) {
    return 1;
  } else if (a.task.name < b.task.name) {
    return -1;
  } else {
    return 0;
  }
}
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
