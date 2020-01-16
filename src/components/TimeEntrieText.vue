<template>
  <div class="text">
    <p class="customer_text">{{ customer }}</p>
    <p class="activity_name_text">{{ activity }}</p>
  </div>
</template>

<script>
export default {
  props: ["customer", "activity"],
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
