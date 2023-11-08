<template>
  <div  v-if="$store.state.drawerOpen">
    <div class="toolbar">
      <span class="md-title">{{ name }}</span>
      <div class="toolbar-end">
        <div class="close_button">
          <YellowButton
            icon-id="close"
            tooltip="Lukk menyen"
            @click="toggleMenu"
          />
        </div>
      </div>
    </div>

    <ul>
      <li
        v-for="item in items"
        :key="item.routeName"
        @click="navTo(item.routeName)"
      >
        <span>{{ item.icon }}</span>

        <span
          :class="{ active: $store.state.currentRoute.name === item.routeName }"
          class="md-list-item-text"
          >{{ item.text }}</span
        >
      </li>
      <li @click="navToAdminpanel">
        <md-icon>admin_panel_settings</md-icon>
        <span class="md-list-item-text">Adminpanel</span>
      </li>
      <li @click="authAction">
        <Icon class="icon" icon-id="meeting_room"></Icon>
        <span class="md-list-item-text">{{ authText }}</span>
      </li>
    </ul>
  </div>
</template>

<script lang="ts">
import {defineComponent} from "vue";
import YellowButton from "@/components/YellowButton.vue";
import authService from "../services/auth";
import config from "@/config";
import { AccountInfo } from "@azure/msal-common";
import Icon from "@/components/Icon.vue";

export default defineComponent({
  components: {
    Icon,
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
          text: "Personlige access token",
          routeName: "tokens",
          icon: "lock_open",
        },
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
.toolbar {

}

.toolbar-end {

}
.close_button {
  margin: 0.4rem 0rem;
}

.active {
  color: #008dcf;
}
</style>
