<template>
  <div class="grid">
    <md-button @click="editFavorites" class="md-icon-button">
      <md-icon class="icon" v-if="selectFavorites">star</md-icon>
      <md-icon class="icon" v-if="!selectFavorites">star_border</md-icon>
    </md-button>
    <h2>{{ day }}</h2>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
export default Vue.extend({
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
