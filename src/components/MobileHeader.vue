<template>
  <div class="grid">
    <EditFavoritesButton />
    <h2>{{ day }}</h2>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import EditFavoritesButton from "./EditFavoritesButton";

export default Vue.extend({
  components: {
    EditFavoritesButton,
  },

  props: ["day"],

  computed: {
    selectFavorites() {
      return this.$store.state.selectFavorites;
    },
  },

  methods: {
    editFavorites() {
      this.$store.commit("TOGGLE_SELECTFAVORITES");
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
