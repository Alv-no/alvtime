<template>
  <div v-if="show" class="center">
    <div class="center_text">
      <md-progress-spinner md-mode="indeterminate"></md-progress-spinner>
      <p>Slapp av. Dataen er nok rett rundt hjørnet nå.</p>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { debounce } from "lodash";

export default {
  data() {
    return {
      show: false,
      timeout: 0,
    };
  },

  created() {
    // @ts-ignore
    this.timeout = setTimeout(() => {
      // @ts-ignore
      this.show = true;
    }, 1000);
  },

  beforeDestroy() {
    // @ts-ignore
    clearTimeout(this.timeout);
  },
};
</script>

<style scoped>
.center {
  display: grid;
  justify-items: center;
  align-items: center;
  height: 90vh;
}

.center_text {
  display: grid;
  justify-items: center;
  align-items: center;
}
</style>
