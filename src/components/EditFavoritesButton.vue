<template>
  <md-button @click="editFavorites" class="md-icon-button">
    <md-icon class="icon" v-if="selectFavorites">star</md-icon>
    <md-icon class="icon" v-if="!selectFavorites">star_border</md-icon>
    <md-tooltip>Velg favoritt aktiviteter</md-tooltip>
  </md-button>
</template>

<script lang="ts">
import Vue from "vue";
export default Vue.extend({
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
