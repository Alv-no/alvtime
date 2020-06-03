<template>
  <CenterColumnWrapper>
    <div>
      <GenerateToken />
      <TokensList />
    </div>
  </CenterColumnWrapper>
</template>

<script lang="ts">
import Vue from "vue";
import TokensList from "@/components/TokensList.vue";
import GenerateToken from "@/components/GenerateToken.vue";
import CenterColumnWrapper from "@/components/CenterColumnWrapper.vue";

export default Vue.extend({
  components: {
    TokensList,
    GenerateToken,
    CenterColumnWrapper,
  },
});
</script>
