<template>
  <div class="unauthorized">
    <Icon icon-id="account_box"></Icon>
    <strong>{{ label }}</strong>
    <p>Logg ut og prøv med en annen bruker eller spør høyere makter om hjelp.</p>
    <YellowButton icon-id="meeting_room" text="Logg ut" @click="logout" />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import YellowButton from "./YellowButton.vue";
import authService from "../services/auth";
import Icon from "@/components/Icon.vue";

export default Vue.extend({
  components: {
    YellowButton,
    Icon,
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
