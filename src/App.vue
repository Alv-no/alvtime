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
