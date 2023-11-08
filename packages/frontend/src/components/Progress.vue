<template>
  <transition name="slide" appear>
    <span v-if="visible">Laster ...</span>
  </transition>
</template>

<script lang="ts">
import {defineComponent} from "vue";
export default defineComponent({
  props: { visible: Boolean },
});
</script>

<style scoped>
.replacement {
  height: 5px;
}

.slide-enter-active {
  transition: all 2s cubic-bezier(0.6, 0, 0.6, 0);
}
.slide-leave-active {
  transition: all 0.8s cubic-bezier(1, 0.5, 0.8, 1);
}
.slide-enter,
.slide-leave-to {
  transform: translateY(-5px);
  opacity: 0;
}
</style>
