<template>
  <div>
    <Progress :visible="progressBarVisible" />
    <div class="grid">
      <button v-if="!$store.state.account" @click="login"
        >Logg inn med SSO</button
      >
    </div>
  </div>
</template>

<script lang="ts">
import {defineComponent} from "vue";
import authService from "../services/auth";
import Progress from "@/components/Progress.vue";

export default defineComponent({
  components: {
    Progress,
  },

  computed: {
    progressBarVisible() {
      return !this.$store.state.tasks.length && !!this.$store.state.account;
    },
  },

  methods: {
    login: () => authService.loginMsal(),
  },
});
</script>

<style scoped>
.grid {
  display: grid;
  justify-content: center;
  align-items: center;
}

.md-button.md-theme-default {
  margin-top: 3rem;
  background-color: #0074c6;
  color: white;
}
</style>
