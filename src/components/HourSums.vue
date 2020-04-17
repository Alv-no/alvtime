<template>
  <div v-if="$store.state.currentRoute.name === 'hours'" class="sums">
    <mq-layout mq="sm">
      <div>{{ daySum }}/7,5</div>
    </mq-layout>
    <div>{{ weekSum }}/37,5</div>
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
    day(): string {
      const str = this.activeDate.format("dddd D. MMMM");
      return str.charAt(0).toUpperCase() + str.slice(1);
    },

    daySum(): string {
      const timeEntries = this.$store.state.timeEntries
        ? this.$store.state.timeEntries
        : [];
      const number = timeEntries.reduce(
        (acc: number, curr: FrontendTimentrie) => {
          if (
            this.activeDate.format(config.DATE_FORMAT) === curr.date &&
            !isNaN(Number(curr.value))
          ) {
            return (acc = acc + Number(curr.value));
          } else {
            return acc;
          }
        },
        0
      );
      const str = number.toString().replace(".", ",");
      return str;
    },

    weekSum(): string {
      const timeEntries = this.$store.state.timeEntries
        ? this.$store.state.timeEntries
        : [];
      return weekTimeEntrieSum(this.activeDate, timeEntries);
    },

    activeDate(): Moment {
      return this.$store.state.activeDate;
    },
  },
});

function weekTimeEntrieSum(
  activeDate: Moment,
  timeEntries: FrontendTimentrie[]
): string {
  const week = createWeek(activeDate).map((date: Moment) =>
    date.format(config.DATE_FORMAT)
  );
  const number = timeEntries.reduce((acc: number, curr: FrontendTimentrie) => {
    if (week.indexOf(curr.date) !== -1 && !isNaN(Number(curr.value))) {
      return (acc = acc + Number(curr.value));
    } else {
      return acc;
    }
  }, 0);
  const str = number.toString().replace(".", ",");
  return str;
}
</script>

<style scoped>
.sums {
  font-size: 0.5rem;
  line-height: 0.7rem;
}
</style>
