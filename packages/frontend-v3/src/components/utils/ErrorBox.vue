<template>
	<div class="error-box-container">
		<slot />
		<button class="close-button" v-if="closable" aria-label="Close" type="button" @click="close">
			<FeatherIcon v-if="closable" name="x" />
		</button>
	</div>
</template>

<script setup lang="ts">
import FeatherIcon from './FeatherIcon.vue';

const { closable } = defineProps<{
	closable?: boolean
}>();

const emit = defineEmits<{
	(event: "close"): void
}>();

const close = () => {
	emit("close");
};
</script>

<style scoped lang="scss">
.error-box-container {
	position: relative;
	background-color: #f8d7da;
	border: 1px solid #f5c6cb;
	border-radius: 15px;
	padding: 12px 16px;
	margin: 16px auto;
	max-width: 600px;
	color: #721c24;
	text-align: center;
	
	.close-button {
		position: absolute;
		top: 8px;
		right: 8px;
		cursor: pointer;
		width: 20px;
		height: 20px;
		color: #721c24;
		transition: color 0.2s ease-in-out;
		padding: 0;
		background: none;
		border: none;
		outline: none;

		&:hover {
			color: #491217;
		}
	}
}
</style>