<template>
	<span>
		<component :is="svgVNode" />
	</span>
</template>
<script setup lang="ts">
import feather from "feather-icons";
import { computed, h } from "vue";

const { name, size } = defineProps({
	name: {
		type: String,
		required: true,
	},
	size: {
		type: Number,
		default: 16,
	},
});

const svgVNode = computed(() => {
	const icon = feather.icons[name as keyof typeof feather.icons];
	if (icon) {
		const svgString = icon.toSvg({ width: size, height: size, class: "feather-icon" });
		const parser = new DOMParser();
		const doc = parser.parseFromString(svgString, "image/svg+xml");
		const svgElement = doc.documentElement;

		// Convert SVG DOM element to VNode
		const attrs: Record<string, string> = {};
		for (const attr of svgElement.attributes) {
			attrs[attr.name] = attr.value;
		}

		return h(
			"svg",
			attrs,
			Array.from(svgElement.childNodes).map((node) => {
				if (node.nodeType === 1) {
					const child = node as Element;
					const childAttrs: Record<string, string> = {};
					for (const attr of child.attributes) {
						childAttrs[attr.name] = attr.value;
					}
					return h(child.tagName, childAttrs);
				}
				return null;
			})
		);
	}
	
	return null;
});

</script>

<style lang="scss">
	.feather-icon {
		position: relative;
		top: 3px;
	}
</style>
