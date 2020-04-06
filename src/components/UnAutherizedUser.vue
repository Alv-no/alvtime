<template>
  <md-empty-state
    md-icon="account_box"
    :md-label="label"
    md-description="Logg ut og prøv med en annen bruker eller spør høyere makter om hjelp."
  >
    <YellowButton @click="logout" iconId="meeting_room" text="Logg ut" />
  </md-empty-state>
</template>

<script lang="ts">
import Vue from "vue";
import YellowButton from "./YellowButton.vue";
import config from "@/config.ts";
import { logout } from "../services/auth";

export default Vue.extend({
  components: {
    YellowButton,
  },

  computed: {
    userName(): string {
      const account = this.$store.state.account;
      return account ? account.userName : "";
    },

    label(): string {
      const userName = this.userName ? this.userName : "Denne AzureAd brukeren";
      return userName + " har ikke tilgang til Alvtime";
    },
  },

  methods: {
    logout() {
      logout();
    },
  },
});
</script>

<style scoped>
.width {
  width: 1rem;
}
</style>
