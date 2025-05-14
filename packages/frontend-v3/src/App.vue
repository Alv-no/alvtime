<template>
  <button @click="loginRedirect">
    Logg inn
  </button>
  <p v-if="isAuthenticated">
    Du er logget inn!
  </p>
  <RouterView :key="$route.path" />
</template>

<script setup lang="ts">
import { useIsAuthenticated } from './composables/useIsAuthenticated';
import { useMsal } from './composables/useMsal';
import { loginRequest } from "./authConfig";

const isAuthenticated = useIsAuthenticated();
const { instance } = useMsal();
    
const loginRedirect = () => {
  instance.loginRedirect(loginRequest);
}
</script>

<style lang="scss" scoped>

</style>
