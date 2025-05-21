<template>
	<span v-html="svgIcon" />
</template>

<script setup lang="ts">
import feather from "feather-icons";
import { computed } from "vue";

const { name, size } = defineProps({
	name: {
		type: String,
		required: true,
	},
	size: {
		type: Number,
		default: 16,
	},
})

const svgIcon = computed(() => {
	const icon = feather.icons[name as keyof typeof feather.icons];
	if (icon) {
		return icon.toSvg({ width: size, height: size, class: "feather-icon" });
	}

	return "";
});
</script>

<style lang="scss">
	.feather-icon {
		position: relative;
		top: 3px;
	}
</style>