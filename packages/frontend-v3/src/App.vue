<template>
	<NavigationBar />
	<div class="wrapper">
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
	padding-bottom: 32px;
	.content-container {
		position: relative;
		max-width: 1200px;
		margin: 0 auto;
		margin-bottom: 20px;
		padding: 16px;
		background-color: $background-color;
		height: 100%;
		border-radius: 25px;
		min-height: 70vh;
		//box-shadow: 0 5px 20px rgba(0, 0, 0, .5);
	}
}
</style>
