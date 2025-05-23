<template>
	<button @click="logOut">
		Logg ut <FeatherIcon name="log-out" />
	</button>
</template>

<script setup lang="ts">
import { useMsal } from "@/composables/useMsal";
import FeatherIcon from "./utils/FeatherIcon.vue";

const { instance } = useMsal();

const logOut = async () => {
	await instance.logoutRedirect();
};
</script>

<style scoped lang="scss">
button {
	background-color: $secondary-color;
	color: $primary-color;
	border-radius: 25px;
	border: none;
	padding: 9px 16px 12px 16px;
	cursor: pointer;
	font-size: 14px;
	font-weight: 600;

	&:hover {
		background-color: $accent-color;
		color: $primary-color;
	}
}
</style>
