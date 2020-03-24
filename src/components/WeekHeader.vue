<template>
  <div class="grid">
    <EditFavoritesButton />
    <md-button @click="onArrowBackClick">
      <md-icon>arrow_back</md-icon>
      <md-tooltip>Gå til forrige uke</md-tooltip>
    </md-button>
    <h2>{{ month }}</h2>
    <md-button @click="onArrowForwardClick">
      <md-icon>arrow_forward</md-icon>
      <md-tooltip>Gå til neste uke</md-tooltip>
    </md-button>
    <div class="sums">
      <div>{{ weekSum }}/37,5</div>
    </div>
    <div />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import EditFavoritesButton from "./EditFavoritesButton.vue";
import { createWeek } from "@/mixins/date";
import { weekSum } from "@/mixins/date";

export default Vue.extend({
  mixins: [weekSum],
  components: {
    EditFavoritesButton,
  },

  computed: {
    week() {
      return createWeek(this.$store.state.activeDate);
    },

    month() {
      let months: string[] = [];
      // @ts-ignore
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
    onArrowBackClick() {
      this.$emit("backClick");
    },
    onArrowForwardClick() {
      this.$emit("forwardClick");
    },
  },
});
</script>

<style scoped>
.grid {
  display: grid;
  grid-template-columns: 107px 70px minmax(6rem, calc(37rem - 140px)) 70px 107px;
  align-items: center;
  justify-content: center;
  text-align: center;
}
</style>
