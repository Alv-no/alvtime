<script setup lang="ts">
import Vue from "vue";

export default Vue.extend({
  props: {
    title: {
      type: String,
      default: "",
    },
    content: {
      type: String,
      default: "",
    },
    confirmText: {
      type: String,
    },
    cancelText: {
      type: String,
    },

  },
});
</script>

<template>

</template>

<style scoped>

</style>