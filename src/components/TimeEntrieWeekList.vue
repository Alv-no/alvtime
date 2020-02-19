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

<script>
import TimeEntrieWeek from "./TimeEntrieWeek";
import TimeEntrie from "./TimeEntrie";
import TimeEntrieText from "./TimeEntrieText.vue";
import config from "@/config";

export default {
  components: {
    TimeEntrieWeek,
    TimeEntrieText,
  },
  props: ["week"],

  computed: {
    tasks() {
      const rows = [...this.tasksWithHours, ...this.tasksWithoutHours].sort(
        sortList
      );
      return rows;
    },

    tasksWithHours() {
      const tasks = [];
      for (const entrie of this.weeksTimeEntries) {
        const task = this.$store.getters.getTask(entrie.taskId);
        if (!tasks.some(t => t.id === task.id)) {
          tasks.push(task);
        }
      }
      return tasks;
    },

    tasksWithoutHours() {
      return this.$store.getters.favoriteTasks.filter(
        task => !this.tasksWithHours.some(t => t.id === task.id)
      );
    },

    weeksTimeEntries() {
      return this.$store.state.timeEntries.filter(entrie =>
        this.isThisWeek(entrie.date)
      );
    },

    daysOfWeek() {
      return this.week.map(day => {
        const d = day.format("ddd DD");
        return d.charAt(0).toUpperCase() + d.slice(1);
      });
    },
  },

  methods: {
    isThisWeek(d) {
      return this.week.some(date => date.format(config.DATE_FORMAT) === d);
    },
  },
};

export function sortList(a, b) {
  if (a.customerName > b.customerName) {
    return 1;
  } else if (a.customerName < b.customerName) {
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

.row {
  display: grid;
  grid-template-columns: minmax(8rem, 16rem) 30rem;
  margin: 0 1rem;
}
</style>
