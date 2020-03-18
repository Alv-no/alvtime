<template>
  <md-snackbar
    :md-position="position"
    :md-duration="duration"
    :md-active.sync="show"
    md-persistent
  >
    <span>{{ text }}</span>
    <SlackButton @click="close" />
    <md-button class="icon_button" @click="close">
      <md-icon class="icon">close</md-icon>
      <md-tooltip>Lukk</md-tooltip>
    </md-button>
  </md-snackbar>
</template>

<script lang="ts">
import Vue from "vue";
import SlackButton from "@/components/SlackButton.vue";

export default Vue.extend({
  components: {
    SlackButton,
  },

  data() {
    return {
      duration: Infinity,
      position: "left",
    };
  },

  created() {
    // @ts-ignore
    window.addError = (errorMessage: string) =>
      this.$store.commit("ADD_TO_ERROR_LIST", errorMessage);
  },

  computed: {
    show: {
      get() {
        return !!this.$store.state.errorTexts.length;
      },
      set() {
        // :md-active.sync="show" neds a setter
      },
    },

    issues() {
      return this.$store.state.errorTexts.join(" - ");
    },

    text() {
      // @ts-ignore
      const maxLength = this.$mq === "sm" ? 70 : 180;
      const isLong = this.issues.length > maxLength;
      if (isLong)
        return (
          // @ts-ignore
          this.issues
            .split("")
            .splice(0, maxLength)
            .join("") + "..."
        );

      // @ts-ignore
      return this.issues;
    },
  },

  methods: {
    close() {
      this.$store.commit("CLEAR_ERROR_LIST");
    },
  },
});
</script>

<style scoped>
.md-snackbar.md-theme-default {
  color: #fff;
  color: var(--md-theme-default-text-primary-on-text-primary, #fff);
  font-weight: 500;
  background-color: #f44336;
}

.md-button.md-theme-default {
  color: #fff;
  color: var(--md-theme-default-primary-on-background, #fff);
}

.md-button.md-theme-default .md-icon-font {
  color: #fff;
  color: var(--md-theme-default-primary-on-background, #fff);
}

.icon_button {
  min-width: 1.5rem !important;
}

.tooltip {
  z-index: 15;
}
</style>
