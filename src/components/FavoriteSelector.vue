<template>
  <div>
    <div class="container">
      <div v-for="task in tasks" :key="task.id" class="row">
        <TimeEntrieText :task="task" />
        <md-checkbox
          :value="!task.favorite"
          type="checkbox"
          :disabled="!isOnline"
          @change="toggleFavorite(task)"
        />
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Task } from "../store/tasks";
import TimeEntrieText from "./TimeEntrieText.vue";
import EditFavoritesButton from "./EditFavoritesButton.vue";

export default Vue.extend({
  components: {
    TimeEntrieText,
    EditFavoritesButton,
  },

  methods: {
    toggleFavorite(task: Task) {
      this.$store.dispatch("PUSH_TASKS", [
        { ...task, favorite: !task.favorite },
      ]);
    },
  },

  computed: {
    tasks() {
      return this.$store.state.tasks;
    },

    isOnline() {
      return this.$store.state.isOnline;
    },
  },
});
</script>

<style scoped>
.row {
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
  color: #000;
  padding: 0 1rem;
}

.container {
  padding-top: 1rem;
}

@media only screen and (min-width: 1000px) {
  .container {
    display: grid;
    justify-content: center;
  }

  .row {
    width: 48rem;
  }
}

.container >>> .md-checkbox.md-theme-default.md-checked .md-checkbox-container {
  background-color: #008dcf;
  border-color: #008dcf;
}

.container >>> .md-checkbox.md-theme-default.md-checked .md-ripple {
  color: #008dcf;
}
</style>
