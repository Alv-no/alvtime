<template>
  <div class="grid">
    <EditFavoritesButton />
    <h2>{{ day }}</h2>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import EditFavoritesButton from "./EditFavoritesButton.vue";

export default Vue.extend({
  components: {
    EditFavoritesButton,
  },

  props: ["day"],
});
</script>

<style scoped>
.grid {
  display: grid;
  grid-template-columns: 0px auto;
  align-items: center;
  text-align: center;
}
</style>
