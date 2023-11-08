<template>
  <li @click="logout">
    <span>meeting_room</span>
    <span class="md-list-item-text">Logg ut</span>
  </li>
</template>

<script lang="ts">
import {defineComponent} from "vue";
import authService from "../services/auth";

export default defineComponent({
  methods: {
    logout() {
      authService.logout();
    },
  },
});
</script>
