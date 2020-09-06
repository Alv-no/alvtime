<template>
  <transition appear name="slide">
    <div v-if="show" class="footer">
      <YellowButton
        tooltip="Gå til dagens dato"
        text="Today"
        @click="onTodayClick"
      />
    </div>
  </transition>
</template>

<script lang="ts">
import Vue from "vue";
import YellowButton from "@/components/YellowButton.vue";

export default Vue.extend({
  components: {
    YellowButton,
  },

  computed: {
    show() {
      return (
        // @ts-ignore
        this.$mq === "sm" &&
        this.$store.state.activeTaskId === -1 &&
        this.$store.state.currentRoute.name === "hours" &&
        !this.$store.state.drawerOpen
      );
    },
  },

  methods: {
    onTodayClick() {
      this.$store.commit("SLIDE_TO_TODAY");
    },
  },
});
</script>

<style scoped>
.footer {
  display: grid;
  justify-content: right;
  align-content: center;
  position: fixed;
  left: 0;
  bottom: 0;
  width: 100%;
  background-color: #00083d;
  color: white;
  text-align: center;
  height: 3rem;
}

/* Enter and leave animations can use different */
/* durations and timing functions.              */
.slide-enter-active {
  transition: all 0.3s ease;
}
.slide-leave-active {
  transition: all 0.3s cubic-bezier(1, 0.5, 0.8, 1);
}
.slide-enter, .slide-leave-to
/* .slide-fade-leave-active below version 2.1.8 */ {
  transform: translateY(3rem);
}
</style>
