<template>
  <div :class="{ mobile }" class="hamburger">
    <YellowButton icon-id="menu" tooltip="Åpne meny" @click="toggleMenu" />
  </div>
</template>

<script lang="ts">
import {defineComponent} from "vue";
import YellowButton from "./YellowButton.vue";
export default defineComponent({
  components: {
    YellowButton,
  },

  computed: {
    mobile(): boolean {
      // @ts-ignore
      return this.$mq == "sm";
    },
  },

  methods: {
    toggleMenu() {
      this.$store.commit("TOGGLE_DRAWER");
    },
  },
});
</script>

<style scoped>
.hamburger {
  margin-right: 1rem;
}

.mobile {
  margin-right: 0.5rem;
}
</style>
