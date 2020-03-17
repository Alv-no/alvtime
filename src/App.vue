<template>
  <div>
    <router-view />
    <UpdateSnackbar />
    <ErrorSnackbar />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import ErrorSnackbar from "@/components/ErrorSnackbar.vue";
import UpdateSnackbar from "@/components/UpdateSnackbar.vue";

export default Vue.extend({
  components: {
    ErrorSnackbar,
    UpdateSnackbar,
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
