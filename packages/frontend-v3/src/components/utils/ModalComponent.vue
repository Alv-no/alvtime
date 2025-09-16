<template>
	<div
		class="modal-overlay"
		@click="handleOverlayClick"
	>
		<div class="modal-content">
			<div class="modal-content-header">
				<h2>{{ title }}</h2>
				<button @click="close">
					<FeatherIcon
						name="x"
						:size="24"
					/>
				</button>
			</div>
			<slot />
		</div>
	</div>
</template>

<script setup lang="ts">
import FeatherIcon from "@/components/utils/FeatherIcon.vue";
const { title = "", clickOutsideToClose } = defineProps<{
	title?: string;
	clickOutsideToClose?: boolean;
}>();

const emit = defineEmits<{
	(event: "close"): void
}>();

const close = () => {
	emit("close");
};

const handleOverlayClick = (event: MouseEvent) => {
	if (clickOutsideToClose && event.target === event.currentTarget) {
		close();
	}
};

</script>

<style scoped lang="scss">
.modal-overlay {
	position: fixed;
	top: 0;
	left: 0;
	right: 0;
	bottom: 0;
	background: rgba(0, 0, 0, 0.5);
	display: flex;
	align-items: center;
	justify-content: center;

	.modal-content {
		background: $background-color;
		max-width: 90%;
		max-height: 90%;
		overflow-y: auto;
		border-radius: 25px;
		min-width: 500px;
		padding: 16px;

		.modal-content-header {
			display: flex;
			justify-content: space-between;
			align-items: center;
			margin-bottom: 16px;
	
			button {
				border-radius: 5px;
				outline: none;
				background-color: transparent;
				color: $primary-color;
				padding: 4px 6px;
				border: none;
	
				&:hover {
					background-color: $secondary-color-light;
				}
	
				&:active {
					background-color: $secondary-color;
				}
			}
		}
	}
}
</style>
