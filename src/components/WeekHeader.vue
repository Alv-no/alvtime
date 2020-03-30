<template>
  <div class="grid">
    <EditFavoritesButton />
    <div class="sums">
      <div>{{ weekSum }}/37,5</div>
    </div>
    <h2>{{ month }}</h2>
    <div>
      <md-button class="arrow_button" @click="onArrowBackClick">
        <md-icon>arrow_back</md-icon>
        <md-tooltip>Gå til forrige uke</md-tooltip>
      </md-button>
      <md-button @click="onTodayClick">Today</md-button>
      <md-button class="arrow_button" @click="onArrowForwardClick">
        <md-icon>arrow_forward</md-icon>
        <md-tooltip>Gå til neste uke</md-tooltip>
      </md-button>
    </div>
    <div />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import EditFavoritesButton from "./EditFavoritesButton.vue";
import { createWeek } from "@/mixins/date";
import { weekTimeEntrieSum } from "@/mixins/date";

export default Vue.extend({
  components: {
    EditFavoritesButton,
  },

  computed: {
    week() {
      return createWeek(this.$store.state.activeDate);
    },

    weekSum(): string {
      return weekTimeEntrieSum(
        this.$store.state.activeDate,
        this.$store.state.timeEntries
      );
    },

    month() {
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
  },

  methods: {
    onArrowBackClick() {
      this.$emit("backClick");
    },
    onArrowForwardClick() {
      this.$emit("forwardClick");
    },
    onTodayClick() {
      this.$emit("todayClick");
    },
  },
});

function upperCase(s: string) {
  return s.charAt(0).toUpperCase() + s.slice(1);
}
</script>

<style scoped>
.grid {
  display: grid;
  grid-template-columns: 130px 70px minmax(6rem, calc(37rem - 205px)) 200px;
  align-items: center;
  justify-content: center;
  text-align: center;
  margin-right: 1rem;
}

.arrow_button {
  min-width: 1rem;
}
</style>
