<template>
  <form novalidate>
    <input
      :class="{ error: hasError }"
      :type="type"
      v-model="value"
      @touchstart="onTouchStart"
      novalidate
    />
  </form>
</template>

<script>
import debounce from "lodash/debounce";

export default {
  props: ["timeEntrie"],
  data() {
    return {
      type: "text",
      hasError: false,
      erroneousValue: 0,
    };
  },

  computed: {
    value: {
      get() {
        if (this.hasError) {
          return this.erroneousValue;
        }
        const { id, date } = this.timeEntrie;
        const entrie = this.$store.getters.getTimeEntrie(id, date);
        return entrie ? entrie.value.toString().replace(".", ",") : 0;
      },
      set: debounce(function(str) {
        this.erroneousValue = str;
        str = str.replace(/,/, ".");
        if (str.match(/[^0-9.]/g)) {
          this.hasError = true;
          return;
        }
        this.hasError = false;
        const value = Number(str);
        const timeEntrie = { ...this.timeEntrie, value };
        this.$store.dispatch("UPDATE_TIME_ENTRIE", timeEntrie);
      }, 2000),
    },
  },

  methods: {
    onTouchStart() {
      this.type = "number";
      setTimeout(() => (this.type = "text"), 200);
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
}

.error {
  border-color: red;
}
</style>
