<template>
  <div class="entry-container">
    <div class="entry-text">
      <span class="customer-name truncate-text">{{ task.project.customer.name }} - {{ task.project.name }}</span>  
      <span class="activity-name truncate-text">{{ task.name }}</span>
    </div>
    <div v-show="showPadlock" class="padlock-container">
      <md-icon class="padlock-icon">lock</md-icon>
    </div>
    <div class="rate-container">
        <small class="rate-text">{{ compensationRatePercentage }}</small>
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
.entry-container {
  font-size: 0.8rem;
  display: flex;
  flex-direction: row;
  justify-content: flex-end;
  min-width: 0;
  margin-right: .4rem;
}

.entry-text {
  display: flex;
  flex-direction: column;
  min-width: 0;
  margin-right: auto;
}

.customer-name {
  margin: 0;
}

.activity-name {
  font-weight: 600;
}

.truncate-text {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.rate-container {
  align-self: center;
  margin-left: .2rem;
}

.rate-text {
  justify-self: right;
  font-weight: lighter;
}

.padlock-container {
  align-self: center;
  
}

.padlock-icon {
  margin-left: .2rem;
}
</style>
