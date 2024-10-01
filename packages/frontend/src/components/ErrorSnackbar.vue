<template>
  <md-snackbar
    :md-position="position"
    :md-duration="duration"
    :md-active.sync="show"
    md-persistent
  >
    <div class="snackbar_content">
      <div class="message_container">
        <span id="error_text_element" class="issues">{{ issueTitle }}</span>
        <span class="issues"> {{ issueText }} </span>
      </div>

      <div class="close_button_container">
        <md-button class="icon_button close_button" @click="close">
          <md-icon class="icon">close</md-icon>
          <Tooltip text="Lukk" />
        </md-button>
      </div>
      <div class="slack_button_container">
        <SlackButton
          class="slack_button"
          tooltip="Kopier feilteksten og åpne Slack"
          @click="onSlackClick"
        />
      </div>
    </div>
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
    issueTitle() {
      const issues = this.$store.getters.getErrorMessages as ErrorResponse[];
      const lastIssue = issues[issues.length - 1];
      console.log("lastIssue", lastIssue);
      const issueCount = issues.length;
      if (lastIssue) {
        return issueCount > 1
          ? `${lastIssue.name} +${issueCount - 1} more error(s)`
          : lastIssue.name;
      }
      return "";
    },
    issueText() {
      const issues = this.$store.getters.getErrorMessages as ErrorResponse[];
      const lastIssue = issues[issues.length - 1];
      console.log("lastIssue", lastIssue);
      const issueCount = issues.length;
      if (lastIssue) {
        return issueCount > 1
          ? `${lastIssue.message} +${issueCount - 1} more error(s)`
          : lastIssue.message;
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

/* quick fix to enable better formatting of the error messages. we just let it go. */
.md-snackbar {
  padding: 0;
  max-height: 80vh;
}

.snackbar_content {
  position: relative;
  width: 100%;
  height: 100%;
  padding: 0.5rem;
  box-sizing: border-box;
}

.message_container {
  margin-right: 2rem;
}

.close_button_container {
  position: absolute;
  top: 0.5rem;
  right: 0.5rem;
}

.slack_button_container {
  position: absolute;
  bottom: 0.5rem;
  right: 0.5rem;
}

.icon_button {
  min-width: 1.5rem !important;
}

.issues {
  display: block; /* or inline-block */
  overflow: hidden;
  line-height: 1.5em;
}

.tooltip {
  z-index: 15;
}
</style>
