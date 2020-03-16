<template>
  <div>
    <router-view />
    <UpdateSnackbar />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import UpdateSnackbar from "@/components/UpdateSnackbar.vue";

export default Vue.extend({
  components: {
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
