<template>
  <div v-if="userFound" class="avatar" @click="toggleMenu">
    <md-avatar class="md-avatar-icon">
      <md-ripple>{{ initial }}</md-ripple>
    </md-avatar>
  </div>
</template>

<script lang="ts">
import Vue from "vue";

export default Vue.extend({
  computed: {
    initial(): string {
      const account = this.$store.state.account;
      const name = account ? account.name : " ";
      return name.split("")[0];
    },

    userFound(): boolean {
      return this.$store.getters.isValidUser;
    },
  },

  methods: {
    toggleMenu() {
      this.$store.commit("TOGGLE_DRAWER");
    },
  },
});
</script>

<style scoped>
.avatar {
  cursor: pointer;
  margin-right: 1rem;
}

.md-avatar.md-theme-default.md-avatar-icon {
  background-color: #f39123;
  color: #fff;
}
</style>
