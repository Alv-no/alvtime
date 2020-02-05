<template>
  <div class="text">
    <p class="customer_text">{{ task.project.customer.name }}</p>
    <p class="activity_name_text">{{ task.name }}</p>
  </div>
</template>

<script>
export default {
  props: ["task"],
};
</script>

<style scoped>
.text {
  display: block;
  line-height: 0.3rem;
  text-align: left;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  margin-right: 1rem;
}

.customer_text {
  font-size: 0.8rem;
}

.activity_name_text {
  font-size: 1rem;
}
</style>
