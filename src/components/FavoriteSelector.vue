<template>
  <div>
    <div class="header">
      <EditFavoritesButton />
      <h2>Select Favorite Tasks</h2>
      <div />
    </div>
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
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Task } from "../store/index";
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
  },
});
</script>

<style scoped>
.header {
  display: grid;
  grid-template-columns: 0 auto;
  align-items: center;
}

.row {
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
  color: #000;
  padding: 0 1rem;
}

@media only screen and (min-width: 1000px) {
  .container {
    display: grid;
    justify-content: center;
  }

  .header {
    grid-template-columns: 107px minmax(16rem, 37rem) 107px;
    justify-content: center;
    text-align: center;
  }

  .row {
    width: 48rem;
  }
}
</style>
