<template>
  <div id="app">
    <router-view />
    <Snackbar />
  </div>
</template>

<script>
import Snackbar from "./components/Snackbar";

export default {
  components: {
    Snackbar,
  },

  mounted() {
    this.fetchTasks();
    this.fetchTimeEntries();
  },

  methods: {
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
