<template>
  <span v-if="iconId"></span>
</template>

<script lang="ts">
import {defineComponent} from "vue";
export default defineComponent({
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