<template>
  <CenterColumnWrapper>
    <div>
      <GenerateToken />
      <TokensList />
    </div>
  </CenterColumnWrapper>
</template>

<script lang="ts">
import {defineComponent} from "vue";
import TokensList from "@/components/TokensList.vue";
import GenerateToken from "@/components/GenerateToken.vue";
import CenterColumnWrapper from "@/components/CenterColumnWrapper.vue";

export default defineComponent({
  components: {
    TokensList,
    GenerateToken,
    CenterColumnWrapper,
  },
});
</script>
