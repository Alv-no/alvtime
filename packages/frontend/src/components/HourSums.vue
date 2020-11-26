<template>
  <div v-if="$store.state.currentRoute.name === 'hours'" class="sums">
    <mq-layout mq="sm">
      <div>{{ daySum }}/{{ dayGoal }}</div>
    </mq-layout>
    <div>{{ weekSum }}/{{ weekGoal }}</div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import config from "@/config";
import { FrontendTimentrie } from "@/store/timeEntries";
import { createWeek } from "@/mixins/date";
import { Moment } from "moment";

export default Vue.extend({
  computed: {
    daySum(): string {
      let sum = 0;
      for (const entrie of this.weeksTimeEntries) {
        if (
          this.activeDate.isSame(entrie.date, "day") &&
          isNumber(entrie.value)
        ) {
          sum = sum + Number(entrie.value);
        }
      }

      return toString(sum);
    },

    dayGoal(): string {
      if (this.isNonWorkDay(this.activeDate)) {
        return "0";
      } else {
        return config.HOURS_IN_WORKDAY.toString();
      }
    },

    weekSum(): string {
      let sum = 0;
      for (const entrie of this.weeksTimeEntries) {
        if (isNumber(entrie.value)) {
          sum = sum + Number(entrie.value);
        }
      }

      return toString(sum);
    },

    weekGoal(): string {
      const week = createWeek(this.mondayOfWeek);

      let goal = 0;
      for (const date of week) {
        if (!this.isNonWorkDay(date)) {
          goal = goal + config.HOURS_IN_WORKDAY;
        }
      }

      return toString(goal);
    },

    weeksTimeEntries(): FrontendTimentrie[] {
      const timeEntries = this.$store.state.timeEntries
        ? this.$store.state.timeEntries
        : [];
      const weekOfStrDates = createWeek(this.mondayOfWeek).map((date: Moment) =>
        date.format(config.DATE_FORMAT)
      );

      return timeEntries.filter(
        (entrie: FrontendTimentrie) =>
          weekOfStrDates.indexOf(entrie.date) !== -1
      );
    },

    mondayOfWeek(): Moment {
      return this.activeDate.clone().startOf("week");
    },

    activeDate(): Moment {
      return this.$store.state.activeDate;
    },
  },

  methods: {
    isNonWorkDay(date: Moment): boolean {
      const isHoliday = this.$store.getters.isHoliday(date);
      const isSunday = date.day() === 0;
      const isSaturday = date.day() === 6;
      return isHoliday || isSunday || isSaturday;
    },
  },
});

function toString(n: number): string {
  return n.toString().replace(".", ",");
}

function isNumber(s: string | number): boolean {
  return !isNaN(Number(s));
}
</script>

<style scoped>
.sums {
  font-size: 0.6rem;
  line-height: 0.7rem;
  text-align: start;
}
</style>
