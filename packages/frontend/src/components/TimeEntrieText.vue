<template>
  <div class="text">
    <p>{{ task.project.customer.name }}</p>
    <p class="activity_name_text">
      {{ task.name }} {{ task.project.name }} 
        <small class="rate-text">{{ compansationRatePercentage }}</small>
    </p>
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
    compansationRatePercentage(): string {
      return `${this.task.compensationRate * 100}%`;
    },
  }
});
</script>

<style scoped>
.text {
  display: block;
  line-height: 0.3rem;
  text-align: left;
  white-space: nowrap;
  overflow: hidden;
  font-size: 0.8rem;
}

.activity_name_text {
  font-weight: 600;
}
.rate-text {
  font-weight: 100;
}

</style>
