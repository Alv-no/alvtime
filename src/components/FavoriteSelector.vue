<template>
  <div class="container">
    <div v-for="task in tasks" :key="task.id" class="row">
      <TimeEntrieText :task="task" />
      <md-checkbox
        :value="!task.favorite"
        type="checkbox"
        @change="toggleFavorite(task)"
      />
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Task } from "../store/index";
import TimeEntrieText from "./TimeEntrieText.vue";

export default Vue.extend({
  components: {
    TimeEntrieText,
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

@media only screen and (min-width: 450px) {
  .container {
    display: grid;
    justify-content: center;
  }

  .row {
    width: 48rem;
  }
}
</style>
