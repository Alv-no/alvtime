<template>
	<div class="status-bar-container">
		<div class="column-box">
			<div class="content-box">
				<HugeiconsIcon
					:icon="Calendar03Icon"
					class="icon"
				/> 22,5/37,5
			</div>
			<div class="content-box flex">
				<div>
					<HugeiconsIcon
						:icon="MoneyBag02Icon"
						class="icon"
					/> 4
				</div>
				<div>
					<HugeiconsIcon
						:icon="BeachIcon"
						class="icon"
					/> 20
				</div>
			</div>
		</div>
	</div>
</template>

<script setup lang="ts">
import { HugeiconsIcon } from "@hugeicons/vue";
import { MoneyBag02Icon, BeachIcon, Calendar03Icon } from "@hugeicons/core-free-icons";
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
			padding: 4px 16px 12px;
			background-color: rgb(206 214 194);

			&.flex {
				display: flex;
				justify-content: space-between;
				gap: 16px;
			}
		}
	}

	.icon {
		position: relative;
		top: 5px;
	}
</style>
