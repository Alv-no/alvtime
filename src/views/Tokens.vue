<template>
  <div>
    <GenerateToken />
    <TokensList />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import TokensList from "@/components/TokensList.vue";
import GenerateToken from "@/components/GenerateToken.vue";

export default Vue.extend({
  components: {
    TokensList,
    GenerateToken,
  },
});
</script>
