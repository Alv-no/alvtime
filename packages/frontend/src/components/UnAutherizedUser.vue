<template>
  <md-empty-state
    md-icon="account_box"
    :md-label="label"
    md-description="Logg ut og prøv med en annen bruker eller spør høyere makter om hjelp."
  >
    <YellowButton icon-id="meeting_room" text="Logg ut" @click="logout" />
  </md-empty-state>
</template>

<script lang="ts">
import Vue from "vue";
import YellowButton from "./YellowButton.vue";
import authService from "../services/auth";

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
      authService.logout();
    },
  },
});
</script>

<style scoped>
.width {
  width: 1rem;
}
</style>
