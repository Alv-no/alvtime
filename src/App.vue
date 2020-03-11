<template>
  <div>
    <router-view />
    <Snackbar />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import Snackbar from "@/components/Snackbar.vue";

export default Vue.extend({
  components: {
    Snackbar,
  },
});
</script>

<style>
html {
  font-size: 20px;
  font-family: "Avenir", Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  color: #2c3e50;
}
</style>
