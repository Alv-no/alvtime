<template>
  <div>
    <button v-if="!isAuthenticated" @click="login">Login</button>
    <div v-if="isAuthenticated">
      <button @click="logout">Logout</button>
      <span>{{ name }}</span>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { logout, login } from "../services/auth";

export default Vue.extend({
  computed: {
    name() {
      const account = this.$store.state.account;
      return account ? account.name : "";
    },

    isAuthenticated() {
      return !!this.$store.state.account;
    },
  },

  methods: {
    login() {
      login();
    },
    logout() {
      logout();
    },
  },
});
</script>
