<template>
  <div>
    <Progress :visible="progressBarVisible" />
    <div class="grid">
      <md-button v-if="!$store.state.account" @click="login"
        >Logg inn med SSO</md-button
      >
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { login } from "../services/auth";
import Progress from "@/components/Progress.vue";

export default Vue.extend({
  components: {
    Progress,
  },

  computed: {
    progressBarVisible() {
      return !this.$store.state.tasks.length && !!this.$store.state.account;
    },
  },

  methods: {
    login: () => login(),
  },
});
</script>

<style scoped>
.grid {
  display: grid;
  justify-content: center;
  align-items: center;
}

.md-button.md-theme-default {
  margin-top: 3rem;
  background-color: #0074c6;
  color: white;
}
</style>
