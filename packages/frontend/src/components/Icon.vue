<template>
  <span v-if="iconId"></span>
</template>

<script setup lang="ts">
import Vue from "vue";
export default Vue.extend({
  props: {
    iconId: {
      type: String,
      default: "",
    },
  },
});
</script>


<style scoped>

</style>