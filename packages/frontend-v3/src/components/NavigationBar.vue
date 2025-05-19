<template>
	<div class="navigation-bar">
		<div class="navigation-container">
			<p>Alvtime</p>
		</div>
	</div>
</template>

<script setup lang="ts">

</script>

<style scoped lang="scss">
.navigation-bar {
	display: flex;
	justify-content: center;
	align-items: center;
	height: 60px;
	background-color: rgb(36, 63, 77);
	color: rgb(248, 241, 227);

	.navigation-container {
		width: 1200px;
		padding: 0 20px;
		display: flex;
		justify-content: flex-start;
		align-items: center;

		a {
			color: white;
			text-decoration: none;
			margin-right: 20px;

			&:hover {
				text-decoration: underline;
			}
		}
	}
}
</style>