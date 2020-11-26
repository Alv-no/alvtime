<template>
  <md-list-item @click="logout">
    <md-icon>meeting_room</md-icon>
    <span class="md-list-item-text">Logg ut</span>
  </md-list-item>
</template>

<script lang="ts">
import Vue from "vue";
import { logout } from "../services/auth";

export default Vue.extend({
  methods: {
    logout() {
      logout();
    },
  },
});
</script>
