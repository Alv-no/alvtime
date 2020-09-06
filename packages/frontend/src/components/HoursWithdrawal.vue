<template>
  <div class="padding">
    <div class="description">
      Her kan du bestille utbetaling av dine akkumulerte overtidstimer
    </div>
    <div class="input-container">
      <Input v-model="hours" :error="!isFloat" placeholder="Antall timer" />
      <YellowButton
        icon-id="add_circle_outline"
        :text="buttonText"
        :disabled="!isNumber"
        @click="onButtonClick"
      />
    </div>
    <div class="list">
      <transition name="expand">
        <div v-if="isNumber" class="row">
          <div class="date">{{ today }}</div>
          <div class="hours">{{ hours }}</div>
        </div>
      </transition>
      <div v-for="order in orders" :key="order.date" class="row">
        <div class="date">{{ order.date }}</div>
        <div class="hours">{{ order.hours }}</div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import YellowButton from "./YellowButton.vue";
import { isFloat } from "@/store/timeEntries";
import Input from "./Input.vue";
import moment, { Moment } from "moment";

export default Vue.extend({
  components: {
    YellowButton,
    Input,
  },

  data() {
    return {
      hours: null,
      ordered: [
        { date: "2020-05-03", hours: 7.5 },
        { date: "2020-04-03", hours: 7 },
        { date: "2020-03-03", hours: 12 },
        { date: "2020-05-02", hours: 5 },
      ],
    };
  },

  computed: {
    orders(): { date: string; hours: number }[] {
      return this.ordered.map(({ date, hours }) => ({
        date: this.formatDate(moment(date)),
        hours,
      }));
    },

    today(): string {
      return this.formatDate(moment());
    },

    isNumber(): boolean {
      return this.isFloat && !!this.hours;
    },

    showHours(): number | null {
      return this.hours ? this.hours : 99;
    },

    buttonText(): string {
      // @ts-ignore
      return this.$mq === "sm" ? "" : "bestill";
    },

    isFloat(): boolean {
      const hours = this.hours ? this.hours : "";
      return isFloat(hours as string);
    },
  },

  methods: {
    onButtonClick() {
      console.log("button clicked");
    },

    formatDate(d: Moment): string {
      const s = d.format("dddd D. MMMM");
      return s.charAt(0).toUpperCase() + s.slice(1);
    },
  },
});
</script>

<style scoped>
.padding {
  padding: 1rem;
}

.input-container {
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
}

.description {
  margin: 0.5rem 0;
}

.hours {
  font-weight: bold;
}

.row {
  display: grid;
  grid-template-columns: 1fr auto;
  padding: 0.5rem 0;
}

.expand-enter-active {
  animation: enter 0.1s;
  animation-timing-function: linear;
}

.expand-leave-active {
  animation: enter 0.1s reverse;
  animation-timing-function: linear;
}

@keyframes enter {
  0% {
    height: 0;
    padding: 0;
    transform: scale(1, 0);
  }
  100% {
    height: 2rem;
    padding: 0.5rem 0;
    transform: scale(1, 1);
  }
}
</style>
