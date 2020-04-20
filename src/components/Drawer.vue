<template>
  <div>
    <md-toolbar class="md-transparent" md-elevation="0">
      <span class="md-title">{{ name }}</span>

      <div class="md-toolbar-section-end">
        <div class="close_button">
          <YellowButton
            icon-id="close"
            tooltip="Lukk menyen"
            @click="toggleMenu"
          />
        </div>
      </div>
    </md-toolbar>

    <md-list>
      <md-list-item @click="navToHours">
        <md-icon>star_border</md-icon>
        <span
          :class="{ active: $store.state.currentRoute.name === 'hours' }"
          class="md-list-item-text"
        >
          Timeføring
        </span>
      </md-list-item>

      <md-list-item @click="navToTasks">
        <md-icon>star_border</md-icon>
        <span
          :class="{ active: $store.state.currentRoute.name === 'tasks' }"
          class="md-list-item-text"
        >
          Favorittaktiviteter
        </span>
      </md-list-item>

      <md-list-item @click="logout">
        <md-icon>meeting_room</md-icon>
        <span class="md-list-item-text">Logg ut</span>
      </md-list-item>
    </md-list>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import YellowButton from "@/components/YellowButton.vue";
import { logout } from "../services/auth";

export default Vue.extend({
  components: {
    YellowButton,
  },

  computed: {
    name(): string {
      const account = this.$store.state.account;
      return account ? account.name : " ";
    },
  },

  methods: {
    toggleMenu() {
      this.$store.commit("TOGGLE_DRAWER");
    },

    navToHours() {
      this.$router.push("hours");
      setTimeout(() => this.$store.commit("TOGGLE_DRAWER"), 150);
    },

    navToTasks() {
      this.$router.push("tasks");
      setTimeout(() => this.$store.commit("TOGGLE_DRAWER"), 150);
    },

    logout() {
      logout();
    },
  },
});
</script>

<style scoped>
.close_button {
  margin: 0.4rem 0.8rem;
}

.active {
  color: #008dcf;
}
</style>
