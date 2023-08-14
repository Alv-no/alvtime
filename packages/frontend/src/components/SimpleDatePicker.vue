<template>
  <div>
    <b>{{ label }}</b><br>
    <input
      class="simple-date-input"
      :value="currentDate"
      type="date"
      @change="onDateChange($event)"
    />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
export default Vue.extend({
  props: {
    defaultDate: {
      type: String,
      default: "",
    },
    label: {
      type: String,
      default: "",
    },
  },
  data() {
    return {
      currentDate: this.defaultDate || "",
    };
  },
  methods: {
    onDateChange(event: InputEvent) {
      const target = event?.target as HTMLInputElement;
      if (!target.value) {
        target.value = this.defaultDate;
      } 

      if (this.currentDate === target.value) {
        return;
      }

      this.currentDate = target.value;
      this.$emit("dateSelected", this.currentDate);
    },
  },
  watch: {
    defaultDate() {
      this.currentDate = this.defaultDate;
    }
  }
});
</script>

<style scoped>
.simple-date-input {
  padding: 0.5rem;
}
</style>
