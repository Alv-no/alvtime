<template>
  <FavoriteSelector />
</template>

<script lang="ts">
import Vue from "vue";
import FavoriteSelector from "@/components/FavoriteSelector.vue";

export default Vue.extend({
  name: "home",
  components: {
    FavoriteSelector,
  },
});
</script>
