<template>
  <div class="containing-block">
    <div class="container">
      <button class="button" @click="onTimeLeftInDayClick">{{ timeLeftInDay }}</button>
    </div>
  </div>
</template>

<script lang="ts">
import {defineComponent} from "vue";
import config from "@/config";

export default defineComponent({
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
  right: 3rem;
}

.containing-block {
  position: relative;
}

.container {
  min-width: 2rem;
  color: inherit;
  border: 2px solid #eabb26;
  border-radius: 4px;
  background-color: #eabb26;
}

.button {
  height: 33px;
  margin: 0;
}

.container .button  {
  background-color: #eabb26;
  padding: 0 12px;
  display: flex;
  justify-content: center;
  align-items: center;
  border: none;
}

.container .button:hover {
  color: black;
  background-color: #eabb26;
  transition: background-color 500ms ease-in-out;
}
</style>
