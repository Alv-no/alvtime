<template>
  <h2>{{ text }}</h2>
</template>

<script lang="ts">
import Vue from "vue";
import { Moment } from "moment";
import { createWeek } from "@/mixins/date";

export default Vue.extend({
  computed: {
    week(): Moment[] {
      return createWeek(this.activeDate);
    },

    text(): string {
      if (!this.$store.getters.isValidUser) return "Alvtime";
      if (this.$store.state.currentRoute.name === "tasks")
        return "Velg aktiviteter";
      if (this.$store.state.currentRoute.name === "tokens")
        return "Personlige access tokens";
      if (this.$store.state.currentRoute.name === "accumulated-hours")
        return "Overtid og ferie";
      if (this.$store.state.currentRoute.name === "dashboard")
        return "Dashboard";
      if (this.$store.state.currentRoute.name === "summarizedhours")
        return "Statistikk";
      // @ts-ignore
      const screenSize = this.$mq;
      if (screenSize === "sm") return this.day;
      if (screenSize !== "sm") return this.month;
      return "";
    },

    month(): string {
      let months: string[] = [];
      let years: string[] = [];
      for (const day of this.week) {
        const month = upperCase(day.format("MMMM"));
        const year = day.format("YYYY");
        if (!months.includes(month)) {
          months = [...months, month];
        }
        if (!years.includes(year)) {
          years = [...years, year];
        }
      }

      let text = `${months[0]} ${years[0]}`;
      if (months.length === 2 && years.length === 2) {
        text = `${months[0]} ${years[0]} - ${months[1]} ${years[1]}`;
      }
      if (months.length === 2 && years.length === 1) {
        text = `${months[0]} - ${months[1]} ${years[0]}`;
      }
      return text;
    },

    day(): string {
      const str = this.activeDate.format("dddd D. MMMM");
      return str.charAt(0).toUpperCase() + str.slice(1);
    },

    activeDate(): Moment {
      return this.$store.state.activeDate;
    },
  },
});

function upperCase(s: string) {
  return s.charAt(0).toUpperCase() + s.slice(1);
}
</script>

<style scoped>
h2 {
  font-weight: 300;
}
</style>
