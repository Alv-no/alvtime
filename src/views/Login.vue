<template>
  <AuthProvider />
</template>

<script lang="ts">
import Vue from "vue";
import AuthProvider from "@/components/AuthProvider.vue";

export default Vue.extend({
  name: "login",
  components: {
    AuthProvider,
  },
});
</script>
