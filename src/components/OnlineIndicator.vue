<template>
  <transition name="fade">
    <div class="offline" v-if="!isOnline">
      Det ser ut som at du har mistet tilgangen til internett.
    </div>
  </transition>
</template>

<script lang="ts">
import Vue from "vue";

export default Vue.extend({
  created() {
    this.$store.commit("UPDATE_ONLINE_STATUS");
    window.addEventListener("online", () =>
      this.$store.commit("UPDATE_ONLINE_STATUS")
    );
    window.addEventListener("offline", () =>
      this.$store.commit("UPDATE_ONLINE_STATUS")
    );
  },

  computed: {
    isOnline() {
      return this.$store.state.isOnline;
    },
  },
});
</script>

<style scoped>
.offline {
  line-height: 40px;
  background-color: #e8b925;
  text-align: center;
  vertical-align: middle;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.5s;
}
.fade-enter, .fade-leave-to /* .fade-leave-active below version 2.1.8 */ {
  opacity: 0;
}
</style>
