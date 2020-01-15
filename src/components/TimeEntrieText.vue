<template>
  <div class="text">
    <p class="customer_text">{{ timeEntrie.customer }}</p>
    <p class="activity_name_text">{{ timeEntrie.activityName }}</p>
  </div>
</template>

<script>
export default {
  props: ["timeEntrie"],
};
</script>

<style scoped>
.text {
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
