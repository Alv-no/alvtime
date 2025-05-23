<template>
	<div class="status-bar-container">
		<div class="column-box">
			<div class="content-box">
				<FeatherIcon
					name="calendar"
					:size="18"
				/> 22,5/37,5
			</div>
			<div class="content-box flex">
				<div>
					<FeatherIcon
						name="database"
						:size="18"
					/> 4
				</div>
				<div>
					<FeatherIcon
						name="sun"
						:size="18"
					/> 20
				</div>
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
			min-width: 90px;
			border-radius: 10px;
			padding: 12px 16px;
			background-color: rgb(206 214 194);

			&.flex {
				display: flex;
				justify-content: space-between;
				gap: 16px;
			}
		}
	}
</style>
