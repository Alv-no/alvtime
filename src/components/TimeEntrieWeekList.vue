<template>
  <div class="center">
    <div class="row">
      <div />
      <div class="days">
        <div v-for="day in daysOfWeek" :key="day">{{ day }}</div>
      </div>
    </div>
    <div class="row" v-for="task in tasks" :key="task.id">
      <TimeEntrieText :task="task" />
      <TimeEntrieWeek :task="task" :week="week" />
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import TimeEntrieWeek from "./TimeEntrieWeek.vue";
import TimeEntrie from "./TimeEntrie.vue";
import TimeEntrieText from "./TimeEntrieText.vue";
import { Task, FrontendTimentrie } from "@/store";
import config from "@/config";
import moment from "moment";

export default Vue.extend({
  components: {
    TimeEntrieWeek,
    TimeEntrieText,
  },
  props: ["week"],

  computed: {
    tasks(): Task[] {
      const rows = [...this.tasksWithHours, ...this.tasksWithoutHours].sort(
        sortList
      );
      return rows;
    },

    tasksWithHours(): Task[] {
      const tasks: Task[] = [];
      for (const entrie of this.weeksTimeEntries) {
        const task = this.$store.getters.getTask(entrie.taskId);
        if (!tasks.some(t => t.id === task.id)) {
          tasks.push(task);
        }
      }
      return tasks;
    },

    tasksWithoutHours(): Task[] {
      return this.$store.getters.favoriteTasks.filter(
        (task: Task) => !this.tasksWithHours.some(t => t.id === task.id)
      );
    },

    weeksTimeEntries(): FrontendTimentrie[] {
      return this.$store.state.timeEntries.filter((entrie: FrontendTimentrie) =>
        this.isThisWeek(entrie.date)
      );
    },

    daysOfWeek(): string[] {
      return this.week.map((day: moment.Moment) => {
        const d = day.format("ddd DD");
        return d.charAt(0).toUpperCase() + d.slice(1);
      });
    },
  },

  methods: {
    isThisWeek(d: string): boolean {
      return this.week.some(
        (date: moment.Moment) => date.format(config.DATE_FORMAT) === d
      );
    },
  },
});

export function sortList(a: Task, b: Task) {
  if (a.project.customer.name > b.project.customer.name) {
    return 1;
  } else if (a.project.customer.name < b.project.customer.name) {
    return -1;
  }

  if (a.name > b.name) {
    return 1;
  } else if (a.name < b.name) {
    return -1;
  } else {
    return 0;
  }
}
</script>

<style scoped>
.days {
  display: grid;
  grid-template-columns: auto auto auto auto auto auto auto;
  gap: 1.51rem;
}

.center {
  display: grid;
  justify-content: center;
}

@media only screen and (max-width: 1000px) {
  .center {
    display: grid;
    justify-content: left;
    overflow: auto;
  }
}

.row {
  display: grid;
  grid-template-columns: minmax(8rem, 16rem) 30rem;
  margin: 0 1rem;
  padding-right: 1rem;
}
</style>
