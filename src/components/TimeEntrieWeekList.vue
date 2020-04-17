<template>
  <div class="center">
    <div class="row">
      <div />
      <div class="days">
        <div v-for="day in daysOfWeek" :key="day">{{ day }}</div>
      </div>
    </div>
    <div v-for="task in tasks" :key="task.id" class="row">
      <TimeEntrieText :task="task" />
      <TimeEntrieWeek :task="task" :week="week" />
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import TimeEntrieWeek from "./TimeEntrieWeek.vue";
import TimeEntrieText from "./TimeEntrieText.vue";
import { Task } from "@/store/tasks";
import { FrontendTimentrie } from "@/store/timeEntries";
import config from "@/config";
import { Moment } from "moment";

export default Vue.extend({
  components: {
    TimeEntrieWeek,
    TimeEntrieText,
  },
  props: { week: { type: Object as () => Moment[], default: () => [] } },

  computed: {
    tasks(): Task[] {
      const rows = [...this.tasksWithHours, ...this.tasksWithoutHours].sort(
        sortList
      );
      if (rows.length <= 4) {
        return rows;
      }
      const activeDate = this.$store.state.activeDate.format(
        config.DATE_FORMAT
      );
      const activeDateIsInWeek = this.week.some(
        (date: Moment) => date.format(config.DATE_FORMAT) === activeDate
      );
      return activeDateIsInWeek ? rows : rows.slice(0, 3);
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
      const timeEntries = this.$store.state.timeEntries
        ? this.$store.state.timeEntries
        : [];
      return timeEntries.filter((entrie: FrontendTimentrie) =>
        this.isThisWeek(entrie.date)
      );
    },

    daysOfWeek(): string[] {
      return this.week.map((day: Moment) => {
        const d = day.format("ddd DD");
        return d.charAt(0).toUpperCase() + d.slice(1);
      });
    },
  },

  methods: {
    isThisWeek(d: string): boolean {
      return this.week.some(
        (date: Moment) => date.format(config.DATE_FORMAT) === d
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
  text-align: center;
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
}
</style>
