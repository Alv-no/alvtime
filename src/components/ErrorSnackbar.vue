<template>
  <md-snackbar
    :md-position="position"
    :md-duration="duration"
    :md-active.sync="show"
    md-persistent
  >
    <span id="error_text_element" class="issues">{{ issues }}</span>
    <SlackButton
      tooltip="Kopier feilteksten og åpne Slack"
      @click="onSlackClick"
    />
    <md-button class="icon_button" @click="close">
      <md-icon class="icon">close</md-icon>
      <md-tooltip class="tooltip">Lukk</md-tooltip>
    </md-button>
  </md-snackbar>
</template>

<script lang="ts">
import Vue from "vue";
import SlackButton from "@/components/SlackButton.vue";

declare global {
  interface Window {
    addError: any;
  }
}

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
      return this.$store.state.errorTexts.join(" -> ");
    },
  },

  methods: {
    onSlackClick() {
      this.$copyText(this.issues);
      this.close();
    },

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
  background-color: #d73125;
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

.issues {
  display: block; /* or inline-block */
  overflow: hidden;
  max-height: 4.4em;
  line-height: 1.5em;
}

.tooltip {
  z-index: 15;
}
</style>
