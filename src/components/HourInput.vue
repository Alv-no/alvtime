<template>
  <form novalidate>
    <button @click="onSevenFiveClick" v-if="showHelperButtons">7,5</button>
    <input
      :class="{ error }"
      type="text"
      v-model="value"
      @touchstart="onTouchStart"
      @blur="onBlur"
      novalidate
      inputmode="decimal"
      ref="inputRef"
    />
  </form>
</template>

<script>
import { defer } from "lodash";
import { isValidInput } from "@/store/timeEntries";

export default {
  props: ["timeEntrie"],
  data() {
    return {
      showHelperButtons: false,
      enableBlur: true,
    };
  },

  computed: {
    inputRef() {
      return this.$refs.inputRef;
    },

    value: {
      get() {
        const entrie = this.$store.getters.getTimeEntrie(this.timeEntrie);
        return entrie ? entrie.value.toString().replace(".", ",") : "0";
      },
      set(str) {
        const timeEntrie = { ...this.timeEntrie, value: str };
        this.$store.dispatch("UPDATE_TIME_ENTRIE", timeEntrie);
      },
    },

    error() {
      return !isValidInput(this.value);
    },
  },

  methods: {
    onTouchStart() {
      setTimeout(() => {
        this.showHelperButtons = true;
        this.inputRef.focus();
      }, 200);
    },

    onBlur() {
      defer(() => {
        if (this.enableBlur && this.showHelperButtons) {
          this.showHelperButtons = false;
        }
        this.enableBlur = true;
      });
    },

    onSevenFiveClick() {
      const timeEntrie = { ...this.timeEntrie, value: 7.5 };
      this.$store.dispatch("UPDATE_TIME_ENTRIE", timeEntrie);
      this.enableBlur = false;
      this.inputRef.focus();
    },
  },
};
</script>

<style scoped>
input {
  appearance: none;
  -moz-appearance: textfield;
  width: 2.1rem;
  padding: 0.4rem;
  font-size: 0.8rem;
  border-radius: 0;
  border: 1px solid black;
}

input:focus {
  outline: none;
}

.error {
  border-color: red;
}

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
