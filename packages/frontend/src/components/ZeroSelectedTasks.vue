<template>
  <md-empty-state
    md-icon="devices_other"
    md-label="Ingen valgte aktiviteter"
    md-description="Trykk på knappen under for å navigere til listen over aktiviteter"
  >
    <YellowButton
      icon-id="arrow_forward"
      tooltip="Gå til aktiviteter"
      text="Aktiviteter"
      @click="navToTasks"
    />
  </md-empty-state>
</template>

<script lang="ts">
import Vue from "vue";
import YellowButton from "./YellowButton.vue";

export default Vue.extend({
  components: {
    YellowButton,
  },

  methods: {
    navToTasks() {
      this.$router.push("tasks");
    },
  },
});
</script>
