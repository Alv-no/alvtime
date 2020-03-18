<template>
  <div>
    <OnlineIndicator />
    <AuthProvider />
    <Swiper v-if="isAuthenticated" />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import OnlineIndicator from "@/components/OnlineIndicator.vue";
import Swiper from "@/components/Swiper.vue";
import AuthProvider from "@/components/AuthProvider.vue";

export default Vue.extend({
  name: "home",
  components: {
    OnlineIndicator,
    Swiper,
    AuthProvider,
  },

  computed: {
    isAuthenticated() {
      return !!this.$store.state.account;
    },
  },
});
</script>
