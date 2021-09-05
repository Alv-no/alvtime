<template>
  <div>
    <TimeLeftInDayButton
      v-if="showHelperButtons"
      :value="value"
      :time-entrie="timeEntrie"
      @click="onTimeLeftInDayClick"
    />
    <input
      ref="inputRef"
      v-model="value"
      :class="{ error, nonZero, hovered }"
      type="text"
      @mouseover="hovered = !(!isOnline || isLocked)"
      @mouseleave="hovered = false"
      novalidate
      inputmode="decimal"
      :disabled="!isOnline || isLocked"
      @input="onInput"
      @touchstart="onTouchStart"
      @blur="onBlur"
      @focus="onFocus"
      @click="onClick"
    />
  </div>
</template>

<script>
import { isFloat } from "@/store/timeEntries";
import config from "@/config";
import TimeLeftInDayButton from "@/components/TimeLeftInDayButton";

export default {
  components: {
    TimeLeftInDayButton,
  },

  props: {
    timeEntrie: { type: Object, default: () => ({}) },
    isLocked: { type: Boolean, default: false },
  },
  data() {
    return {
      showHelperButtons: false,
      enableBlur: true,
      localValue: "0",
      hovered: false,
    };
  },

  computed: {
    inputRef() {
      return this.$refs.inputRef;
    },

    value: {
      get() {
        if (this.$store.state.editing) return this.localValue;
        const obj =
          this.$store.state.timeEntriesMap[
            `${this.timeEntrie.date}${this.timeEntrie.taskId}`
          ];
        return obj && obj.value ? obj.value.toString().replace(".", ",") : "0";
      },
      set(str) {
        this.localValue = str;
        const validStr = str.replace(/^[,.]/, "0,").replace(".", ",");
        const timeEntrie = { ...this.timeEntrie, value: validStr };
        this.$store.dispatch("UPDATE_TIME_ENTRIE", timeEntrie);
      },
    },

    nonZero() {
      return Number(this.value.replace(",", "."));
    },

    inputHover() {
      return !this.isOnline || this.isLocked;
    },

    error() {
      return !isFloat(this.value);
    },

    isOnline() {
      return this.$store.state.isOnline;
    },

    activeDate() {
      return this.$store.state.activeDate;
    },
  },

  watch: {
    value() {
      this.localValue = this.value;
    },

    activeDate() {
      const isSameTask =
        this.$mq === "sm" &&
        this.activeDate.format(config.DATE_FORMAT) === this.timeEntrie.date &&
        this.$store.state.activeTaskId === this.timeEntrie.taskId;
      if (isSameTask) {
        this.inputRef.focus();
        this.showHelperButtons = true;
      }
    },
  },

  mounted() {
    this.localValue = this.value;
  },

  methods: {
    onTouchStart(e) {
      e.target.focus();
    },

    onBlur() {
      this.$store.commit("UPDATE_EDITING", false);
      if (this.activeDate.format(config.DATE_FORMAT) === this.timeEntrie.date) {
        this.$store.commit("UPDATE_ACTVIE_TASK", -1);
      }
      setTimeout(() => {
        if (this.enableBlur && this.showHelperButtons) {
          this.showHelperButtons = false;
        }
        this.enableBlur = true;
      }, 200);
    },

    onFocus() {
      this.$store.commit("UPDATE_ACTVIE_TASK", this.timeEntrie.taskId);
      this.localValue = this.value;
    },

    onClick() {
      this.showHelperButtons = true;
      this.inputRef.setSelectionRange(0, this.inputRef.value.length);
    },

    onInput() {
      this.$store.commit("UPDATE_EDITING", true);
    },

    onTimeLeftInDayClick() {
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
  border-radius: 5px;
  border: 1px solid #e0e0e0;
  background-color: #f7f7f7;
}

input:focus {
  outline: none;
}

.hovered {
  border-color: #008dcf;
  transition: border-color 500ms ease-out;
}

.error {
  background-color: #d7312540;
  transition: border-color 500ms ease-in-out;
}

.nonZero {
  border-color: #000;
}
</style>
