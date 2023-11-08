<template>
  <div class="button">
    <YellowButton
      v-if="isTasks"
      :toggle="$store.state.selectFavorites"
      icon-id="star"
      icon-id2="star_border"
      tooltip="Velg favorittaktiviteter"
      text="Favoritter"
      @click="editFavorites"
    />
  </div>
</template>

<script lang="ts">
import {defineComponent} from "vue";
import YellowButton from "./YellowButton.vue";

export default defineComponent({
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
