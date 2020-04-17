<template>
  <div class="slide">
    <div v-for="row in rows" :key="row.task.id" class="grid">
      <TimeEntrieText :task="row.task" />
      <HourInput :time-entrie="row.timeEntrie" />
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import TimeEntrieText from "./TimeEntrieText.vue";
import HourInput from "./HourInput.vue";
import config from "@/config";
import { Task } from "@/store/tasks";
import { FrontendTimentrie } from "@/store/timeEntries";
import { Moment } from "moment";

interface Row {
  task: Task;
  timeEntrie: FrontendTimentrie;
}

export default Vue.extend({
  components: {
    TimeEntrieText,
    HourInput,
  },
  props: {
    date: {
      type: Object as () => Moment,
      default: () => {
        return {} as Moment;
      },
    },
  },

  computed: {
    rows(): Row[] {
      return [...this.rowsWithHours, ...this.rowsWithoutHours].sort(sortList);
    },

    rowsWithHours(): Row[] {
      return this.daysTimeEntries.map((entrie: FrontendTimentrie) => {
        const task = this.$store.getters.getTask(entrie.taskId);
        return this.createRow(task, entrie);
      });
    },

    rowsWithoutHours(): Row[] {
      return this.$store.getters.favoriteTasks
        .filter((task: Task) => !this.isTaskInEntries(task))
        .map((task: Task) => this.createRow(task));
    },

    daysTimeEntries(): FrontendTimentrie[] {
      const timeEntries = this.$store.state.timeEntries
        ? this.$store.state.timeEntries
        : [];
      return timeEntries.filter((entrie: FrontendTimentrie) =>
        this.isThisDate(entrie.date)
      );
    },
  },

  methods: {
    isTaskInEntries(task: Task): boolean {
      return this.daysTimeEntries.some(
        (entrie: FrontendTimentrie) => entrie.taskId === task.id
      );
    },

    isThisDate(date: string): boolean {
      return date === this.date.format(config.DATE_FORMAT);
    },

    createRow(task: Task, timeEntrie?: FrontendTimentrie): Row {
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
.slide {
  min-height: calc(100vh - 75px);
}

.grid {
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
  color: #000;
  padding: 0 1rem;
}
</style>
