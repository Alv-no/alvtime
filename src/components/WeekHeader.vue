<template>
  <div class="grid">
    <EditFavoritesButton />
    <h2>{{ month }}</h2>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import EditFavoritesButton from "./EditFavoritesButton.vue";

export default Vue.extend({
  components: {
    EditFavoritesButton,
  },
  props: ["week"],

  computed: {
    month() {
      let months: string[] = [];
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
});
</script>

<style scoped>
.grid {
  display: grid;
  grid-template-columns: 0px auto;
  align-items: center;
}
</style>
