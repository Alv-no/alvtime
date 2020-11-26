<template>
  <div class="containing-block">
    <div class="container">
      <md-button @click="onTimeLeftInDayClick">{{ timeLeftInDay }}</md-button>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import config from "@/config";

export default Vue.extend({
  props: {
    timeEntrie: {
      type: Object,
      default: function() {
        return {};
      },
    },
    value: { type: String, default: "" },
  },

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
.container {
  position: absolute;
  left: -46px;
}

.containing-block {
  position: relative;
}

.container >>> .md-button.md-theme-default {
  min-width: 2rem;
  color: inherit;
  border: 2px solid #eabb26;
  border-radius: 30px;
  background-color: white;
}

.md-button {
  height: 33px;
  margin: 0;
}

.container >>> .md-button .md-ripple {
  padding: 0 0px;
  display: flex;
  justify-content: center;
  align-items: center;
}

.container >>> .md-button.md-theme-default:hover {
  color: black;
  background-color: #eabb26;
  transition: background-color 500ms ease-in-out;
}
</style>
