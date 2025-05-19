<template>
	<NavigationBar />
	<RouterView :key="$route.path" />
</template>

<script setup lang="ts">
import NavigationBar from './components/NavigationBar.vue';
</script>

<style lang="scss" scoped>

</style>
