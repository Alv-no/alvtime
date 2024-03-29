<template>
  <md-snackbar
    :md-position="position"
    :md-duration="duration"
    :md-active.sync="show"
    md-persistent
  >
    <span id="error_text_element" class="issues">{{ issueText }}</span>
    <SlackButton
      tooltip="Kopier feilteksten og åpne Slack"
      @click="onSlackClick"
    />
    <md-button class="icon_button" @click="close">
      <md-icon class="icon">close</md-icon>
      <Tooltip text="Lukk" />
    </md-button>
  </md-snackbar>
</template>

<script lang="ts">
import Vue from "vue";
import { ErrorResponse } from "../services/httpClient";
import SlackButton from "@/components/SlackButton.vue";
import Tooltip from "@/components/Tooltip.vue";

declare global {
  interface Window {
    addError: any;
  }
}

export default Vue.extend({
  components: {
    SlackButton,
    Tooltip,
  },

  data() {
    return {
      duration: Infinity,
      position: "left",
    };
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
      return this.$store.getters.getErrorMessages;
    },
    issueText() {
      const issues = this.$store.getters.getErrorMessages as ErrorResponse[];
      const lastIssue = issues[issues.length - 1];
      const issueCount = issues.length;
      if (lastIssue) {
        return issueCount > 1
          ? `${lastIssue.name} +${issueCount - 1} more error(s)`
          : lastIssue.name;
      }
      return "";
    },
  },

  created() {
    window.addError = (errorMessage: string) =>
      this.$store.commit("ADD_TO_ERROR_LIST", errorMessage);
  },

  methods: {
    onSlackClick() {
      this.$copyText(this.$store.getters.getAllErrors);
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
  font-weight: 500;
  background-color: #d73125;
}

.md-button.md-theme-default {
  color: #fff;
}

.md-button.md-theme-default .md-icon-font {
  color: #fff;
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
