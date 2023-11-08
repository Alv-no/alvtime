<template>
  <div>
    <div class="toolbar">
      <div class="toolbar-row">
        <div class="toolbar-section toolbar-section-start">
          <router-link to="/">
            <div class="logo_grid" @click="openGapestokk">
              <img
                class="light logo"
                src="/img/logo_white.svg"
                alt="Hvit Alv-logo"
              />
            </div>
          </router-link>
        </div>
        <div class="toolbar-section toolbar-section-end">
          <invoice-rate v-if="userFound" />
          <hamburger v-if="userFound" />
        </div>
      </div>
      <div class="toolbar-row">
        <CenterColumnWrapper>
          <Toolbar />
        </CenterColumnWrapper>
      </div>
    </div>

    <Drawer />

    <div class="app-content">
      <router-view />
      <UpdateSnackbar />
      <OnlineSnackbar />
      <ErrorSnackbar />
    </div>

    <DayFooter />
  </div>
</template>

<script lang="ts">
import {defineComponent} from "vue";
import moment from "moment";
import ErrorSnackbar from "@/components/ErrorSnackbar.vue";
import UpdateSnackbar from "@/components/UpdateSnackbar.vue";
import OnlineSnackbar from "@/components/OnlineSnackbar.vue";
import Toolbar from "@/components/Toolbar.vue";
import Hamburger from "@/components/Hamburger.vue";
import Drawer from "@/components/Drawer.vue";
import DayFooter from "@/components/DayFooter.vue";
import CenterColumnWrapper from "@/components/CenterColumnWrapper.vue";
import InvoiceRate from "@/components/InvoiceRate.vue";

export default defineComponent({
  components: {
    ErrorSnackbar,
    UpdateSnackbar,
    OnlineSnackbar,
    Toolbar,
    Hamburger,
    Drawer,
    DayFooter,
    CenterColumnWrapper,
    InvoiceRate,
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

    userFound(): boolean {
      return this.$store.getters.isValidUser;
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

  methods: {
    openGapestokk() {
      this.$store.commit("SET_DONT_SHOW_GAPESTOKK", false);
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
body {
  font-size: 14px;
  letter-spacing: .01em;
  margin: 0;
}
</style>

<style scoped>
.md-app {
  height: calc(100vh);
}

.toolbar {
  background-color: #00083d;
  color: white;
  padding: 0;
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
.toolbar-row {
  gap: 1rem;
  min-height: 64px;
  display: flex;
}

.toolbar {
  min-height: 96px;
  background: #00083d;
}

.toolbar-section {
  display: flex;
  align-items: center;
  flex: 1;
}
.toolbar-section-start {
  justify-content: flex-start;
  order: 0;
}
.toolbar-section-end {
  justify-content: flex-start;
  order: 1;
}

</style>
