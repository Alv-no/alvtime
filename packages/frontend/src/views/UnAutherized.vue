<template>
  <UnAutherizedUser />
</template>

<script lang="ts">
import {defineComponent} from "vue";
import UnAutherizedUser from "@/components/UnAutherizedUser.vue";

export default defineComponent({
  components: {
    UnAutherizedUser,
  },
});
</script>
