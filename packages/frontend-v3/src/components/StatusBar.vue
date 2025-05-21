<template>
	<div class="status-bar-container">
		<div class="column-box">
			<div class="content-box">
				<FeatherIcon
					name="calendar"
					:size="18"
				/> 22,5/37,5
			</div>
			<div class="content-box">
				<FeatherIcon
					name="database"
					:size="18"
				/> 4
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import FeatherIcon from "@/components/utils/FeatherIcon.vue";
</script>

<style scoped lang="scss">
	.status-bar-container {
		background-color: rgb(157, 172, 134);
		border-radius: 15px;
		width: 100%;
		display: flex;
		justify-content: flex-start;
		align-items: flex-start;

		.column-box {
			display: flex;
			flex-direction: column;
			justify-content: center;
			align-items: flex-start;
			padding: 16px;
			gap: 8px;
		}

		.content-box {
			border-radius: 10px;
			padding: 12px;
			background-color: rgb(206 214 194);
		}
	}
</style>