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
        <TimeEntrieText
          :task="task"
          :is-expanded="task.id === selectedEntryKey"
          @expand-entry="onExpandEntry"
        />
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
import { Task } from "@/store/tasks";
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
      selectedEntryKey: -1,
    };
  },

  computed: {
    tasks(): Task[] {
      if (this.searchphrase.length === 0) {
        return this.$store.state.tasks;
      }
      const fuse = new Fuse<Task>(this.$store.state.tasks, {
        keys: ["name", "project.name", "project.customer.name"],
      });
      const result = fuse.search(this.searchphrase);
      return result.map((x: { item: Task }) => x.item);
    },

    isOnline() {
      return this.$store.state.isOnline;
    },
  },
  created() {
    this.$store.dispatch("FETCH_TASKS");
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
    onExpandEntry(id: number) {
      // If selected entry is same as currently selected, reset to close entry
      if (this.selectedEntryKey === id) {
        this.selectedEntryKey = -1;
      } else {
        this.selectedEntryKey = id;
      }
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
