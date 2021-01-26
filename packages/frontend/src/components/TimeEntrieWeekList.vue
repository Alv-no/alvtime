<template>
  <CenterColumnWrapper>
    <div class="center">
      <div class="row">
        <div class="week-number">Uke {{ weekNumber }}</div>
        <div class="days">
          <div v-for="day in week" :key="day._d.Date">
            <DayPill :date="day" />
          </div>
        </div>
      </div>
      <ZeroSelectedTasks v-if="tasks.length < 1" />
      <div v-for="task in tasks" :key="task.id" class="row">
        <TimeEntrieText :task="task" />
        <TimeEntrieWeek :task="task" :week="week" />
      </div>
    </div>
  </CenterColumnWrapper>
</template>

<script lang="ts">
import Vue from "vue";
import TimeEntrieWeek from "./TimeEntrieWeek.vue";
import TimeEntrieText from "./TimeEntrieText.vue";
import DayPill from "./DayPill.vue";
import ZeroSelectedTasks from "./ZeroSelectedTasks.vue";
import { Task } from "@/store/tasks";
import { FrontendTimentrie } from "@/store/timeEntries";
import config from "@/config";
import { Moment } from "moment";
import CenterColumnWrapper from "@/components/CenterColumnWrapper.vue";

export default Vue.extend({
  components: {
    TimeEntrieWeek,
    TimeEntrieText,
    DayPill,
    CenterColumnWrapper,
    ZeroSelectedTasks,
  },
  props: { week: { type: Array as () => Moment[], default: () => [] } },

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

    weekNumber(): number {
      return this.week[0].week();
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
  grid-template-columns: repeat(7, auto);
  gap: 1.51rem;
  text-align: center;
  margin-bottom: 0.55rem;
}

.center {
  padding-top: 1rem;
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
  grid-template-columns: minmax(8rem, 999rem) 30rem;
  margin: 0 1rem;
}

.week-number {
  display: grid;
  align-items: center;
  margin-bottom: 0.55rem;
  font-size: 0.8rem;
}
</style>
