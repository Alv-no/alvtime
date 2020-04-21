<template>
  <div>
    <md-app md-waterfall md-mode="fixed-last">
      <md-app-toolbar class="app-toolbar md-large md-dense">
        <div class="md-toolbar-row">
          <div class="md-toolbar-section-start">
            <router-link to="/">
              <div class="logo_grid">
                <img
                  class="light logo"
                  src="/img/logo_white.svg"
                  alt="Hvit Alv-logo"
                />
                <div class="logo_text">time</div>
              </div>
            </router-link>
          </div>
          <div class="md-toolbar-section-end">
            <Avatar />
          </div>
        </div>

        <div class="md-toolbar-row">
          <Toolbar />
        </div>
      </md-app-toolbar>

      <md-app-drawer :md-active.sync="$store.state.drawerOpen" md-right>
        <Drawer />
      </md-app-drawer>
      <md-app-content>
        <div>
          <Progress :visible="progressBarVisible" />
          <router-view />
          <UpdateSnackbar />
          <OnlineSnackbar />
          <ErrorSnackbar />
        </div>
      </md-app-content>
    </md-app>
    <DayFooter />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import moment from "moment";
import ErrorSnackbar from "@/components/ErrorSnackbar.vue";
import UpdateSnackbar from "@/components/UpdateSnackbar.vue";
import OnlineSnackbar from "@/components/OnlineSnackbar.vue";
import Toolbar from "@/components/Toolbar.vue";
import Avatar from "@/components/Avatar.vue";
import Drawer from "@/components/Drawer.vue";
import DayFooter from "@/components/DayFooter.vue";
import Progress from "@/components/Progress.vue";

export default Vue.extend({
  components: {
    ErrorSnackbar,
    UpdateSnackbar,
    OnlineSnackbar,
    Toolbar,
    Avatar,
    Drawer,
    DayFooter,
    Progress,
  },

  data() {
    return {
      pageLoadTime: moment(),
    };
  },

  computed: {
    isBecomingActive(): boolean {
      const { oldState, newState } = this.interactionState;
      return oldState === "passive" && newState === "active";
    },

    thirtyMinutesSinceLastPageLoad(): boolean {
      return moment().diff(this.pageLoadTime, "minutes") > 30;
    },

    interactionState(): { oldState: string; newState: string } {
      return this.$store.state.interactionState;
    },

    progressBarVisible() {
      return (
        this.$store.state.tasks.length && this.$store.state.timeEntries === null
      );
    },
  },

  watch: {
    interactionState() {
      if (
        isIPhone() &&
        this.isBecomingActive &&
        this.thirtyMinutesSinceLastPageLoad
      ) {
        location.reload();
      }
    },
  },
});

function isIPhone() {
  return /iPhone/i.test(navigator.userAgent);
}
</script>

<style>
html {
  font-size: 20px;
  font-family: source-sans-pro, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
}
</style>

<style scoped>
.md-app {
  height: calc(100vh);
}

.md-toolbar.md-theme-default {
  background-color: #00083d;
  color: white;
}

.md-app-content {
  padding: 0;
  padding-top: 0;
  padding-bottom: 3rem;
}

.logo {
  width: 3rem;
  margin-left: 3rem;
}

@media only screen and (max-width: 650px) {
  .logo {
    margin-left: 1rem;
  }
}

@media only screen and (max-width: 449px) {
  .logo {
    margin-left: 0.5rem;
  }
}

.app-toolbar {
  z-index: 8;
}

.logo_grid {
  display: grid;
  grid-template-columns: auto auto;
}

.logo_text {
  color: white;
}
</style>
