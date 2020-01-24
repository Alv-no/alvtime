<template>
  <form novalidate>
    <button @click="onSevenFiveClick" v-if="showHelperButtons">7,5</button>
    <input
      :class="{ error: hasError }"
      type="text"
      :value="value"
      @input="debouncedValueSet"
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

export default {
  props: ["timeEntrie"],
  data() {
    return {
      hasError: false,
      rawVaue: 0,
      showHelperButtons: false,
      enableBlur: true,
      valueSetTimeout: 0,
    };
  },

  computed: {
    inputRef() {
      return this.$refs.inputRef;
    },

    value() {
      if (this.hasError) {
        return this.rawValue;
      }
      const { id, date } = this.timeEntrie;
      const entrie = this.$store.getters.getTimeEntrie(id, date);
      return entrie ? entrie.value.toString().replace(".", ",") : 0;
    },
  },

  methods: {
    debouncedValueSet({ target: { value: str } }) {
      if (this.valueSetTimeout) {
        clearTimeout(this.valueSetTimeout);
      }
      this.valueSetTimeout = setTimeout(() => {
        this.valueSet(str);
      }, 2000);
    },

    valueSet(str) {
      this.rawValue = str;
      str = str.replace(/,/, ".");
      if (!this.isValidInput(str)) {
        this.hasError = true;
        return;
      }
      this.hasError = false;

      const value = Number(str);
      const timeEntrie = { ...this.timeEntrie, value };
      this.$store.dispatch("UPDATE_TIME_ENTRIE", timeEntrie);
    },

    isValidInput(str) {
      const isMaxOneComma = str.match(/[.,]/g)
        ? str.match(/[.,]/g).length <= 1
        : true;
      const isOnlyDigitsAndComma = !str.match(/[^0-9.,]/g);
      return isOnlyDigitsAndComma && isMaxOneComma;
    },

    onTouchStart() {
      setTimeout(() => {
        this.showHelperButtons = true;
        this.inputRef.focus();
      }, 200);
    },

    onBlur() {
      defer(() => {
        if (this.enableBlur) {
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
