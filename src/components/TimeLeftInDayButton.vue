<template>
  <button @click="onTimeLeftInDayClick">
    {{ timeLeftInDay }}
  </button>
</template>

<script lang="ts">
import Vue from "vue";
import config from "@/config";

export default Vue.extend({
  props: ["timeEntrie", "value"],

  computed: {
    timeLeftInDay(): string {
      let timeLeft = config.HOURS_IN_WORKDAY;
      for (let entrie of this.$store.state.timeEntries) {
        if (
          entrie.date === this.timeEntrie.date &&
          entrie.taskId !== this.timeEntrie.taskId
        ) {
          timeLeft = timeLeft - Number(entrie.value);
        }
      }

      timeLeft = timeLeft > 0 ? timeLeft : config.HOURS_IN_WORKDAY;
      const timeLeftStr = timeLeft.toString().replace(".", ",");
      return timeLeftStr === this.value ? "0" : timeLeftStr;
    },
  },

  methods: {
    onTimeLeftInDayClick() {
      const timeEntrie = { ...this.timeEntrie, value: this.timeLeftInDay };
      this.$store.dispatch("UPDATE_TIME_ENTRIE", timeEntrie);
      this.$emit("click");
    },
  },
});
</script>

<style scoped>
button {
  background-color: #e8b925;
  border: none;
  padding: 0.3rem 0.4rem;
  text-align: center;
  text-decoration: none;
  display: inline-block;
  font-size: 0.8rem;
  margin-right: 0.4rem;
}
</style>
