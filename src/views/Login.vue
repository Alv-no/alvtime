<template>
  <div class="grid">
    <md-button v-if="!$store.state.account" @click="login">Login with Azure Ad</md-button>
    <Spinner v-if="!$store.state.tasks.length && $store.state.account" />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { login } from "../services/auth";
import Spinner from "@/components/Spinner.vue";

export default Vue.extend({
  components: {
    Spinner,
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
