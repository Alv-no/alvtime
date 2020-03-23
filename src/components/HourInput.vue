<template>
  <div>
    <button @click="onTimeLeftInDayClick" v-if="showHelperButtons">
      {{ timeLeftInDay }}
    </button>
    <input
      :class="{ error }"
      type="text"
      v-model="value"
      @touchstart="onTouchStart"
      @blur="onBlur"
      @focus="onFocus"
      novalidate
      inputmode="decimal"
      ref="inputRef"
      :disabled="!isOnline"
    />
  </div>
</template>

<script>
import { defer } from "lodash";
import { isFloat } from "@/store/timeEntries";
import config from "@/config";

export default {
  props: ["timeEntrie"],
  data() {
    return {
      showHelperButtons: false,
      enableBlur: true,
      localValue: "0",
      editing: false,
    };
  },

  computed: {
    inputRef() {
      return this.$refs.inputRef;
    },

    value: {
      get() {
        if (this.editing) return this.localValue;
        const entrie = this.$store.getters.getTimeEntrie(this.timeEntrie);
        return entrie ? entrie.value.toString().replace(".", ",") : "0";
      },
      set(str) {
        this.localValue = str.replace(".", ",");
        const timeEntrie = { ...this.timeEntrie, value: str };
        this.$store.dispatch("UPDATE_TIME_ENTRIE", timeEntrie);
      },
    },

    error() {
      return !isFloat(this.value);
    },

    timeLeftInDay() {
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
      return timeLeft.toString().replace(".", ",");
    },

    isOnline() {
      return this.$store.state.isOnline;
    },

    activeDate() {
      return this.$store.state.activeDate;
    },
  },

  watch: {
    activeDate() {
      const isSameTask =
        this.activeDate.format(config.DATE_FORMAT) === this.timeEntrie.date &&
        this.$store.state.activeTaskId === this.timeEntrie.taskId;
      if (isSameTask) {
        this.inputRef.focus();
        this.showHelperButtons = true;
      }
    },
  },

  methods: {
    onTouchStart(e) {
      setTimeout(() => {
        const isValueChanged = this.value !== this.timeLeftInDay;
        if (isValueChanged) {
          this.showHelperButtons = true;
          this.$store.commit("UPDATE_ACTVIE_TASK", this.timeEntrie.taskId);
          e.target.focus();
        }
      }, 200);
    },

    onBlur() {
      this.editing = false;
      defer(() => {
        if (this.enableBlur && this.showHelperButtons) {
          this.showHelperButtons = false;
          if (
            this.activeDate.format(config.DATE_FORMAT) === this.timeEntrie.date
          ) {
            this.$store.commit("UPDATE_ACTVIE_TASK", -1);
          }
        }
        this.enableBlur = true;
      });
    },

    onFocus() {
      this.localValue = this.value;
      this.editing = true;
    },

    onTimeLeftInDayClick() {
      const timeEntrie = { ...this.timeEntrie, value: this.timeLeftInDay };
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
