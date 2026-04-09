<template>
	<button
		:class="{
			'link-button': linkButton,
			'disabled': disabled,
			'icon-only': iconOnly,
			'icon-left': iconLeft,
			'icon-right': iconRight,
			'small': isMobile
		}"
		:disabled="disabled"
	>
		<slot />
	</button>
</template>

<script setup lang="ts">
const { linkButton, disabled, iconOnly, iconLeft, iconRight } = defineProps<{
	linkButton?: boolean;
	disabled?: boolean;
	iconOnly?: boolean;
	iconLeft?: boolean;
	iconRight?: boolean;
}>();

const isMobile = window.innerWidth <= 768;
</script>

<style lang="scss" scoped>
button {
	background-color: $secondary-color;
	color: $primary-color;
	border-radius: 25px;
	border: none;
	padding: 9px 16px 12px 16px;
	cursor: pointer;
	font-size: 14px;
	font-weight: 600;
	text-wrap: nowrap;

	&.small {
		padding: 6px 12px 9px 12px;
		font-size: 12px;
	}

	&.icon-left {
		padding: 9px 16px 12px 12px;
	}

	&.icon-right {
		padding: 9px 12px 12px 16px;
	}

	&:hover {
		background-color: $secondary-color-light;
		color: $primary-color;
	}

  &.disabled,
  &:disabled {
    opacity: 0.5;
    cursor: not-allowed;
    pointer-events: none;
  }
}
</style>
