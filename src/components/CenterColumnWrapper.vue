<template>
  <div class="outer-container">
    <div class="inner-container">
      <slot></slot>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";

export default Vue.extend({});
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
