<template>
  <div class="text">
    <p class="customer_text">{{ task.project.customer.name }}</p>
    <p class="activity_name_text">{{ task.name }}</p>
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
});
</script>

<style scoped>
.text {
  display: block;
  line-height: 0.3rem;
  text-align: left;
  white-space: nowrap;
  overflow: hidden;
}

.customer_text {
  font-size: 0.8rem;
}

.activity_name_text {
  font-size: 1rem;
}
</style>
