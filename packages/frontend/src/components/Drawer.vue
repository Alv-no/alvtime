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
        <md-icon>query_builder</md-icon>

        <span
          :class="{ active: $store.state.currentRoute.name === item.routeName }"
          class="md-list-item-text"
          >{{ item.text }}</span
        >
      </md-list-item>
      <md-list-item @click="navToAdminpanel">
        <md-icon>admin_panel_settings</md-icon>
        <span class="md-list-item-text">Adminpanel</span>
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
import { logout, login } from "../services/auth";

export default Vue.extend({
  components: {
    YellowButton,
  },

  data() {
    return {
      items: [
        { text: "Timeføring", routeName: "hours" },
        { text: "Favorittaktiviteter", routeName: "tasks" },
        { text: "Overtid og avspasering", routeName: "accumulated-hours" },
        { text: "Personal access tokens", routeName: "tokens" },
      ],
    };
  },

  computed: {
    name(): string {
      return this.account && this.account.name ? this.account.name : "";
    },

    authText(): string {
      return this.account ? "Logg ut" : "Logg in";
    },

    account(): Account {
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
        logout();
      } else {
        login();
      }
    },

    navToAdminpanel() {
      if (process.env.NODE_ENV === 'development') {
        window.open('http://localhost:3000/adminpanel/');
      } else {
        window.open('https://alvtime-admin-react-pwa-as.azurewebsites.net/adminpanel');
      }
    }
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
