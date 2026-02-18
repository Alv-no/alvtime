<template>
	<div class="card">
		<h3 class="card-title">
			{{ title }}
		</h3>
		<b class="card-value"> {{ value }}</b>
	</div>
</template>

<script setup lang="ts">
defineProps<{
  title: string
  value: string | number
}>();
</script>

<style scoped lang="scss">
.card {
  padding: 0.2rem 0.5rem 1.0rem 0.5rem;
  display: flex;
  flex-direction: column;
  border: 3px solid $secondary-color;
  border-radius: 0.5rem;
  margin-bottom: 0.5rem;
}

.card-title {
  color: $primary-color;
  font-weight: normal;
  text-align: center;
}

.card-value {
  text-align: center
}
</style>