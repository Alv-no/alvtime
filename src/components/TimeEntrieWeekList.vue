<template>
  <div>
    <div class="container">
      <div class="grid">
        <div />
        <p v-for="day in daysOfWeek" :key="day">{{ day }}</p>
      </div>
      <TimeEntrieWeek
        v-for="task in tasks"
        :key="task.id"
        :task="task"
        :week="week"
      />
    </div>
  </div>
</template>

<script>
import TimeEntrieWeek from "./TimeEntrieWeek";
import TimeEntrie from "./TimeEntrie";
import config from "@/config";

export default {
  components: {
    TimeEntrieWeek,
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

    month() {
      let months = [];
      for (const day of this.week) {
        const month = day.format("MMMM");
        const upperCasedMonth = month.charAt(0).toUpperCase() + month.slice(1);
        if (!months.includes(upperCasedMonth)) {
          months = [...months, upperCasedMonth];
        }
      }
      return months.join(" - ");
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
.grid {
  display: grid;
  grid-template-columns: minmax(8rem, 15rem) auto auto auto auto auto auto auto;
  gap: 2.6rem;
  padding: 0 2rem;
}

.container {
  display: grid;
  justify-content: center;
}

p {
  width: 1.85rem;
  height: 1rem;
  white-space: nowrap;
}
</style>
