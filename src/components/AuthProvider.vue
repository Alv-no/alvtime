<template>
  <div>
    <button :disabled="!isOnline" v-if="!isAuthenticated" @click="login">
      Login
    </button>
    <div v-if="isAuthenticated">
      <button  @click="reload">Reload</button>
      <button :disabled="!isOnline" @click="logout">Logout</button>
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

    isOnline() {
      return this.$store.state.isOnline;
    },
  },

  created() {
    if (!this.isAuthenticated) {
      this.login();
    }
  },

  methods: {
    login() {
      login();
    },
    logout() {
      logout();
    },
    reload() {
      location.reload()
    }
  },
});
</script>
