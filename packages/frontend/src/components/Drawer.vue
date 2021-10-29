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
      <md-list-item
        v-for="item in items"
        :key="item.routeName"
        @click="navTo(item.routeName)"
      >
        <md-icon>{{ item.icon }}</md-icon>

        <span
          :class="{ active: $store.state.currentRoute.name === item.routeName }"
          class="md-list-item-text"
          >{{ item.text }}</span
        >
      </md-list-item>
      <md-list-item @click="authAction">
        <md-icon>meeting_room</md-icon>
        <span class="md-list-item-text">{{ authText }}</span>
      </md-list-item>
    </md-list>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import YellowButton from "@/components/YellowButton.vue";
import authService from "../services/auth";
import config from "@/config";
import { AccountInfo } from "@azure/msal-common";

export default Vue.extend({
  components: {
    YellowButton,
  },

  data() {
    return {
      items: [
        { text: "Timeføring", routeName: "hours", icon: "query_builder" },
        { text: "Aktiviteter", routeName: "tasks", icon: "local_activity" },
        { text: "Statistikk", routeName: "summarizedhours", icon: "insights" },
        {
          text: "Overtid og ferie",
          routeName: "accumulated-hours",
          icon: "watch_later",
        },
        {
          text: "Dashboard",
          routeName: "dashboard",
          icon: "bar_chart",
        },
        {
          text: "Personlige access token",
          routeName: "tokens",
          icon: "lock_open",
        },
        {
          text: "Admin Panel",
          routeName: "admin",
          icon: "admin_panel_settings"
        }
      ],
    };
  },

  computed: {
    name(): string {
      return this.account && this.account.name ? this.account.name : "";
    },

    authText(): string {
      return this.account ? "Logg ut" : "Logg inn";
    },

    account(): AccountInfo {
      return this.$store.state.account;
    },
  },

  methods: {
    toggleMenu() {
      this.$store.commit("TOGGLE_DRAWER");
    },

    navTo(routeName: string) {
      this.$router.push(routeName);
      setTimeout(() => this.$store.commit("TOGGLE_DRAWER"), 150);
    },

    authAction() {
      if (this.account) {
        authService.logout();
      } else {
        authService.loginMsal();
      }
    },

    navToAdminpanel() {
      window.open(config.BASE_URL_ADMINPANEL);
    },
  },
});
</script>

<style scoped>
.close_button {
  margin: 0.4rem 0rem;
}

.active {
  color: #008dcf;
}
</style>
