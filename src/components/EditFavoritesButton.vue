<template>
  <div class="button">
    <YellowButton
      v-if="isTasks"
      @click="editFavorites"
      :toggle="$store.state.selectFavorites"
      iconId="star"
      iconId2="star_border"
      tooltip="Velg favoritt aktiviteter"
      text="Favoritter"
    />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import YellowButton from "./YellowButton.vue";

export default Vue.extend({
  components: {
    YellowButton,
  },

  computed: {
    isTasks() {
      return !!this.$store.state.tasks.length;
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
.button {
  margin: 0 1rem;
}
</style>
