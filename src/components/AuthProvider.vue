<template>
  <div>
    <button @click="login">Login</button>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
export default Vue.extend({
  data() {
    return {
      account: null,
    };
  },

  methods: {
    login() {
      this.$store.dispatch("LOGIN");
    },
  },
});
</script>
