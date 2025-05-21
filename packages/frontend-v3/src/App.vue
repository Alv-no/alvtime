<template>
	<NavigationBar />
	<div class="wrapper">
		<div class="blue-bar" />
		<div class="content-container">
			<RouterView :key="$route.path" />
		</div>
	</div>
</template>

<script setup lang="ts">
import NavigationBar from "./components/NavigationBar.vue";
</script>

<style lang="scss" scoped>
.wrapper {
	position: relative;

	.blue-bar {
		background-color: $primary-color;
		z-index: 4;
		position: absolute;
		height: 400px;
		width: 100%;
		top: 0;
		left: 0;
	}

	.content-container {
		position: relative;
		max-width: 1200px;
		margin: 0 auto;
		padding: 16px;
		z-index: 6;
		background-color: $background-color;
		height: 100%;
		border-radius: 25px;
		min-height: 70vh;
	}
}

</style>
