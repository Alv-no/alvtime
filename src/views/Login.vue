<template>
  <div class="grid">
    <md-button @click="login">Login with Azure Ad</md-button>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { login } from "../services/auth";

export default Vue.extend({
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
  background-color: #0074c6;
  color: white;
}
</style>
