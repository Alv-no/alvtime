<template>
  <UnAutherizedUser />
</template>

<script lang="ts">
import Vue from "vue";
import UnAutherizedUser from "@/components/UnAutherizedUser.vue";

export default Vue.extend({
  components: {
    UnAutherizedUser,
  },
});
</script>
