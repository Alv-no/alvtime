<template>
  <div v-if="userFound" class="avatar" @click="toggleMenu">
    <md-avatar class="md-avatar-icon">
      <md-ripple>
        <img
          v-if="!useInitial"
          :src="imageUrl"
          alt="Profile picture"
          @error="onError"
        />
        <div v-if="useInitial">{{ initial }}</div>
      </md-ripple>
    </md-avatar>
  </div>
</template>

<script lang="ts">
import Vue from "vue";

export default Vue.extend({
  data() {
    return {
      useInitial: false,
    };
  },

  computed: {
    initial(): string {
      const account = this.$store.state.account;
      const name = account ? account.name : " ";
      return name.split("")[0];
    },

    imageUrl(): string {
      const account = this.$store.state.account;
      const name = account ? "Jan Tore Bøe".replace(/ /g, "_") : "";
      return `https://files-cdn.vitaminw.no/5be7d601f81143448bde804302723901/Root/Bilder/profilbilde_${name}_512.png`;
    },

    userFound(): boolean {
      return this.$store.getters.isValidUser;
    },
  },

  methods: {
    toggleMenu() {
      this.$store.commit("TOGGLE_DRAWER");
    },

    onError(e: any) {
      this.useInitial = true;
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
