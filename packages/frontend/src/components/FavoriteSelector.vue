<template>
  <CenterColumnWrapper>
    <div class="list">
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
  </CenterColumnWrapper>
</template>

<script lang="ts">
import Vue from "vue";
import { Task } from "../store/tasks";
import TimeEntrieText from "./TimeEntrieText.vue";
import CenterColumnWrapper from "@/components/CenterColumnWrapper.vue";

export default Vue.extend({
  components: {
    TimeEntrieText,
    CenterColumnWrapper,
  },

  computed: {
    tasks() {
      return this.$store.state.tasks;
    },

    isOnline() {
      return this.$store.state.isOnline;
    },
  },

  methods: {
    toggleFavorite(task: Task) {
      this.$store.dispatch("PUSH_TASKS", [
        { ...task, favorite: !task.favorite },
      ]);
    },
  },
});
</script>

<style scoped>
.row {
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
  padding: 0 1rem;
}

.list {
  padding-top: 1rem;
}

.list >>> .md-checkbox.md-theme-default.md-checked .md-checkbox-container {
  background-color: #008dcf;
  border-color: #008dcf;
}

.list >>> .md-checkbox.md-theme-default.md-checked .md-ripple {
  color: #008dcf;
}
</style>
