<template>
	<AlvtimeButton
		iconRight
		@click="logOut"
	>
		Logg ut <FeatherIcon name="log-out" />
	</AlvtimeButton>
</template>

<script setup lang="ts">
import { useMsal } from "@/composables/useMsal";
import FeatherIcon from "./utils/FeatherIcon.vue";
import AlvtimeButton from "./utils/AlvtimeButton.vue";

const { instance } = useMsal();

const logOut = async () => {
	await instance.logoutRedirect();
};
</script>