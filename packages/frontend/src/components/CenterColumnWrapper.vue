<template>
  <div class="outer-container">
    <div class="inner-container">
      <slot></slot>
    </div>
  </div>
</template>

<script lang="ts">
import {defineComponent} from "vue";

export default defineComponent({});
</script>

<style scoped>
.outer-container {
  display: grid;
  justify-content: center;
  width: 100%;
}

.inner-container {
  display: grid;
  grid-template-columns: minmax(1rem, 50rem);
}
</style>
