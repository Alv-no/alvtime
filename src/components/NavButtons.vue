<template>
  <div v-if="show">
    <YellowButton
      @click="onArrowBackClick"
      iconId="arrow_back"
      tooltip="Gå til forrige uke"
    />
    <YellowButton
      @click="onTodayClick"
      tooltip="Gå til dagens dato"
      text="Today"
    />
    <YellowButton
      @click="onArrowForwardClick"
      iconId="arrow_forward"
      tooltip="Gå til neste uke"
    />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import YellowButton from "./YellowButton.vue";

export default Vue.extend({
  components: {
    YellowButton,
  },

  computed: {
    show(): boolean {
      // @ts-ignore
      const screenSize = this.$mq;
      return screenSize !== "sm" && this.$store.state.currentRoute.path === "/hours";
    },
  },

  methods: {
    onTodayClick() {
      this.$store.commit("SLIDE_TO_THIS_WEEK");
    },

    onArrowForwardClick() {
      this.$store.commit("SLIDE_NEXT");
    },

    onArrowBackClick() {
      this.$store.commit("SLIDE_PREV");
    },
  },
});

function upperCase(s: string) {
  return s.charAt(0).toUpperCase() + s.slice(1);
}
</script>
