<template>
  <div>
    <md-progress-bar
      v-if="visible"
      class="{hidden}"
      md-mode="indeterminate"
    ></md-progress-bar>
    <div v-if="!visible" class="replacement" />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
export default Vue.extend({
  props: ["visible"],
});
</script>

<style scoped>
.replacement {
  height: 5px;
}
</style>
