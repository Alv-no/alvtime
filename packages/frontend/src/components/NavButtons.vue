<template>
  <div v-if="show">
    <YellowButton
      icon-id="arrow_back"
      tooltip="Gå til forrige uke"
      @click="onArrowBackClick"
    />
    <YellowButton
      tooltip="Gå til dagens dato"
      text="Today"
      @click="onTodayClick"
    />
    <YellowButton
      icon-id="arrow_forward"
      tooltip="Gå til neste uke"
      @click="onArrowForwardClick"
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
      return (
        screenSize !== "sm" && this.$store.state.currentRoute.path === "/hours"
      );
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
</script>
