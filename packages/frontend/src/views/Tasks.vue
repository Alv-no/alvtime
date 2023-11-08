<template>
  <FavoriteSelector />
</template>

<script lang="ts">
import {defineComponent} from "vue";
import FavoriteSelector from "@/components/FavoriteSelector.vue";

export default defineComponent({
  name: "Home",
  components: {
    FavoriteSelector,
  },
});
</script>
