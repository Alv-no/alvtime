<template>
  <CenterColumnWrapper>
    <div class="list">
      <div class="search">
        <Input v-model="searchphrase" placeholder="Søk i listen" />
        <YellowButton
          icon-id="clear"
          tooltip="Fjern søk"
          :disabled="searchphrase.length < 1"
          @click="clear"
        />
      </div>
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
import Input from "./Input.vue";
import YellowButton from "./YellowButton.vue";
import CenterColumnWrapper from "@/components/CenterColumnWrapper.vue";
import Fuse from "fuse.js";

export default Vue.extend({
  components: {
    TimeEntrieText,
    CenterColumnWrapper,
    Input,
    YellowButton,
  },

  data() {
    return {
      searchphrase: "",
      fuse: new Fuse(this.$store.state.tasks, {
        keys: ["name", "project.name", "project.customer.name"],
      }),
    };
  },

  computed: {
    tasks(): Task[] {
      if (this.searchphrase.length === 0) {
        return this.$store.state.tasks;
      }
      const result = (this.fuse.search(this.searchphrase) as unknown) as {
        item: Task;
      }[];
      return result.map((x: { item: Task }) => x.item);
    },

    isOnline() {
      return this.$store.state.isOnline;
    },
  },

  methods: {
    clear() {
      this.searchphrase = "";
    },
    toggleFavorite(task: Task) {
      this.$store.dispatch("PUSH_TASKS", [
        { ...task, favorite: !task.favorite },
      ]);
    },
  },
});
</script>

<style scoped>
.search {
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
  padding: 0 1rem;
  padding-bottom: 0.5rem;
}

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
