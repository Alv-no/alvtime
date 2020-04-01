<template>
  <div>
    <TimeLeftInDayButton
      @click="onTimeLeftInDayClick"
      :value="value"
      :timeEntrie="timeEntrie"
      v-if="showHelperButtons"
    />
    <input
      :class="{ error }"
      type="text"
      @input="onInput"
      v-model="value"
      @touchstart="onTouchStart"
      @blur="onBlur"
      @focus="onFocus"
      novalidate
      inputmode="decimal"
      ref="inputRef"
      @click="onClick"
      :disabled="!isOnline"
    />
  </div>
</template>

<script>
import { defer } from "lodash";
import { isFloat } from "@/store/timeEntries";
import config from "@/config";
import TimeLeftInDayButton from "@/components/TimeLeftInDayButton";

export default {
  components: {
    TimeLeftInDayButton,
  },

  props: ["timeEntrie"],
  data() {
    return {
      showHelperButtons: false,
      enableBlur: true,
      localValue: "0",
    };
  },

  mounted() {
    this.localValue = this.value;
  },

  computed: {
    inputRef() {
      return this.$refs.inputRef;
    },

    value: {
      get() {
        if (this.$store.state.editing) return this.localValue;
        const obj = this.$store.state.timeEntriesMap[
          `${this.timeEntrie.date}${this.timeEntrie.taskId}`
        ];
        return obj && obj.value ? obj.value.toString().replace(".", ",") : "0";
      },
      set(str) {
        const validStr = str.replace(/^[,.]/, "0,");
        this.localValue = validStr.replace(".", ",");
        const timeEntrie = { ...this.timeEntrie, value: validStr };
        this.$store.dispatch("UPDATE_TIME_ENTRIE", timeEntrie);
      },
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
      this.showHelperButtons = true;
      this.$store.commit("UPDATE_ACTVIE_TASK", this.timeEntrie.taskId);
      e.target.focus();
    },

    onBlur() {
      this.$store.commit("UPDATE_EDITING", false);
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
    },

    onClick() {
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
  border-radius: 0;
  border: 1px solid black;
}

input:focus {
  outline: none;
}

.error {
  border-color: red;
}
</style>
