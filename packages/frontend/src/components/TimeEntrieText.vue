<template>
  <div class="entry">
    <p class="customer-name">{{ task.project.customer.name }}</p>
    <div class="activity-name-container">
      <p class="activity-name">{{ task.name }} {{ task.project.name }}</p>
      <small class="rate-text">{{ compensationRatePercentage }}</small>
    </div>
    <div
      v-show="showPadlock"
      v-bind:class="{
        'padlock-left': activityView,
        'padlock-right': !activityView,
      }"
    >
      <md-icon class="icon">lock</md-icon>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Task } from "@/store/tasks";

export default Vue.extend({
  props: {
    task: {
      type: Object as () => Task,
      default: (): Task => {
        return {} as Task;
      },
    },
    activityView: { type: Boolean, default: false },
  },
  computed: {
    compensationRatePercentage(): string {
      return `${this.task.compensationRate * 100}%`;
    },
    showPadlock(): boolean {
      return this.task.locked;
    },
  },
});
</script>

<style scoped>
.entry {
  white-space: nowrap;
  overflow: hidden;
  font-size: 0.8rem;
}

.customer-name {
  margin: 0;
}

.activity-name-container {
  display: flex;

  /* Prevent percentage values from touching the hour input */
  padding-right: 0.5rem;
  float: left;
}

.activity-name {
  font-weight: 600;
  margin: 0;

  overflow: hidden;
  text-overflow: ellipsis;
}

.rate-text {
  font-weight: lighter;
  margin-left: 0.25rem;
}

.padlock-right {
  margin-right: 0.4rem;
  float: right;
}
.padlock-left {
  margin-right: 0.4rem;
  float: left;
}
.icon {
  position: relative;
  bottom: 0.4rem;
}
</style>
