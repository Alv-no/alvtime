<template>
  <div
    class="day"
    :class="{ 'day-off': isDayOff, holiday: isHoliday }"
    @mouseover="mouseOver"
    @mouseleave="mouseLeave"
  >
    <div v-if="hover || !holiday">{{ text }}</div>
    <div v-if="!hover && holiday" class="holiday-text">{{ text }}</div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Moment } from "moment";

export default Vue.extend({
  props: {
    date: {
      type: Object as () => Moment,
      default: {} as Moment,
    },
  },

  data() {
    return {
      hover: false,
    };
  },

  computed: {
    text(): string {
      const d = this.date.format("ddd D");
      const text = this.holiday && !this.hover ? this.holiday : d;
      const upperCased = text.charAt(0).toUpperCase() + text.slice(1);
      return upperCased;
    },

    holiday(): string {
      return this.$store.getters.getHoliday(this.date);
    },

    isDayOff(): boolean {
      return this.isSunday || this.isSaturday;
    },

    isHoliday(): boolean {
      return this.$store.getters.isHoliday(this.date);
    },

    isSunday(): boolean {
      return this.date.day() === 0;
    },

    isSaturday(): boolean {
      return this.date.day() === 6;
    },
  },

  methods: {
    mouseOver() {
      this.hover = true;
    },

    mouseLeave() {
      this.hover = false;
    },
  },
});
</script>

<style scoped>
.day {
  display: grid;
  align-content: center;
  background-color: #008dcf;
  color: white;
  height: 1.55rem;
  width: 2.8rem;
  padding: 0.1rem;
  border-radius: 5px;
  font-size: 0.7rem;
  line-height: 1.5rem;
}

.day-off {
  background-color: #eabb26;
  color: black;
}

.holiday {
  background-color: #f39123;
  color: black;
}

.holiday-text {
  font-size: 0.5rem;
  line-height: 0.7rem;
  overflow-wrap: break-word;
  hyphens: auto;
  overflow-y: hidden;
}
</style>
