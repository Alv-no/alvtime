<script lang="ts">
import {defineComponent} from "vue";

export default defineComponent({
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
<div class="confirmation-dialog">
  <h3>{{ title }}</h3>
</div>
</template>

<style scoped>

</style>