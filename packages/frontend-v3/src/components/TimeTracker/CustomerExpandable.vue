<template>
	<div class="customer-container">
		<div class="customer-info-wrapper">
			<p class="customer-name">
				Alv
			</p>
			<p class="customer-project">
				Interntid
			</p>
		</div>
		<div class="customer-status-wrapper">
			<div class="customer-stats">
				Denne uken: 22,5t | Denne måneden: 145,5t
			</div>
			<FeatherIcon
				name="chevron-down"
				:size="36"
			/>
		</div>
	</div>
</template>

<script setup lang="ts">
import FeatherIcon from "../utils/FeatherIcon.vue";
</script>

<style scoped lang="scss">
.customer-container {
	display: flex;
	justify-content: space-between;
	align-items: center;
	padding: 4px 16px;
	margin-top: 16px;
	border: 1px solid rgb(206, 214, 194, 0.5);
	border-radius: 10px;
	min-height: 70px;

	.customer-info-wrapper {
		display: flex;
		flex-direction: column;
		justify-content: center;
		align-items: flex-start;
		gap: 2px;
		margin: 8px 0;
	}
	

	.customer-name {
		font-size: 1.5rem;
		font-weight: 600;
		margin: 0;
	}

	.customer-project {
		font-size: 1.2rem;
		font-weight: 400;
		margin: 0;
	}

	.customer-status-wrapper {
		display: flex;
		justify-content: flex-end;
		align-items: center;
		gap: 16px;

		.customer-stats {
			background-color: rgb(206, 214, 194);
			border-radius: 10px;
			padding: 12px 12px 9px;
		}
	}
}
</style>