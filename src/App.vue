<template>
  <div id="app">
    <button @click="login">Login</button>
    <button @click="logout">Logout</button>
    <router-view />
    <Snackbar />
  </div>
</template>

<script>
import Snackbar from "./components/Snackbar";
import { msalApp, GRAPH_REQUESTS } from "./services/auth";

export default {
  components: {
    Snackbar,
  },

  mounted() {
    this.fetchTasks();
    this.fetchTimeEntries();
  },

  methods: {
    async login() {
      const loginResponse = await msalApp.loginPopup(GRAPH_REQUESTS.LOGIN);
      console.log("loginResponse: ", loginResponse);
    },

    logout() {
      msalApp.logout();
    },

    fetchTasks() {
      return this.$store.dispatch("FETCH_TASKS");
    },

    fetchTimeEntries() {
      return this.$store.dispatch("FETCH_TIME_ENTRIES");
    },
  },
};
</script>

<style>
html {
  font-size: 20px;
}
#app {
  font-family: "Avenir", Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
}
</style>
