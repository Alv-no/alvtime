<template>
  <li @click="logout">
    <span>meeting_room</span>
    <span class="md-list-item-text">Logg ut</span>
  </li>
</template>

<script lang="ts">
import Vue from "vue";
import authService from "../services/auth";

export default Vue.extend({
  methods: {
    logout() {
      authService.logout();
    },
  },
});
</script>
