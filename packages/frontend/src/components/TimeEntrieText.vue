<template>
  <div class="entry">
    <p class="customer-name">{{ task.project.customer.name }}</p>
    <div class="activity-name-container">
      <p class="activity-name">{{ task.name }} {{ task.project.name }}</p>
      <small class="rate-text">{{ compensationRatePercentage }}</small>
    </div>
    <div v-show="showPadlock" class="padlock">
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

.padlock {
  margin-right: 0.4rem;
  float: right;
}
.icon {
  position: relative;
  bottom: 0.5rem;
}
</style>
