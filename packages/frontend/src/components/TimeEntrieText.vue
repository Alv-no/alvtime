<template>
  <div
    class="entry-container"
    :class="zoomClass"
    @click="$emit('expand-entry', task.id)"
  >
    <div v-if="!isExpanded" class="entry-text">
      <span class="customer-name truncate-text"
        >{{ task.project.customer.name }} - {{ task.project.name }}</span
      >
      <span class="activity-name truncate-text">{{ task.name }}</span>
    </div>
    <div v-if="isExpanded" class="entry-text-expanded">
      <label class="expanded-label">
        Kunde:
        <span class="expanded-text">{{ task.project.customer.name }}</span>
      </label>
      <label class="expanded-label">
        Prosjekt:
        <span class="expanded-text">{{ task.project.name }}</span>
      </label>
      <label class="expanded-label">
        Oppgave:
        <span class="expanded-text">{{ task.name }}</span>
      </label>
      <label class="expanded-label">
        Rate:
        <small class="expanded-text">{{ compensationRatePercentage }}</small>
      </label>
       <label class="expanded-label">
        Beskrivelse:
        <small class="expanded-text">{{ task.description }}</small>
      </label>
    </div>
    <div v-show="showPadlock" class="padlock-container">
      <md-icon class="padlock-icon">lock</md-icon>
    </div>
    <div v-if="!isExpanded" class="rate-container">
      <small class="rate-text">{{ compensationRatePercentage }}</small>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Task } from "@/store/tasks";

export default Vue.extend({
  props: {
    task: {
      type: Object as () => Task,
      default: (): Task => {
        return {} as Task;
      },
    },
    isExpanded: Boolean,
  },
  computed: {
    compensationRatePercentage(): string {
      return `${this.task.compensationRate * 100}%`;
    },
    showPadlock(): boolean {
      return this.task.locked;
    },
    zoomClass(): string {
      return this.isExpanded ? "zoom-out" : "zoom-in";
    },
  },
});
</script>

<style scoped>
.entry-container {
  font-size: 0.8rem;
  display: flex;
  flex-direction: row;
  justify-content: flex-end;
  min-width: 0;
  margin-right: 0.4rem;
}

.zoom-in {
  cursor: zoom-in;
}

.zoom-out {
  cursor: zoom-out;
}
.entry-text {
  display: flex;
  flex-direction: column;
  min-width: 0;
  margin-right: auto;
}

.entry-text-expanded {
  display: flex;
  flex-direction: column;
  min-width: 0;
  margin-right: auto;
  margin-top: 0.4rem;
  margin-bottom: 0.4rem;
  word-break: break-word;
}

.expanded-label {
  font-weight: 600;
  cursor: zoom-out;
}
.expanded-text {
  font-weight: normal;
  font-size: 0.8rem;
  cursor: zoom-out;
}

.customer-name {
  margin: 0;
}

.activity-name {
  font-weight: 600;
}

.truncate-text {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.rate-container {
  align-self: center;
  margin-left: 0.2rem;
}

.rate-text {
  justify-self: right;
  font-weight: lighter;
}

.padlock-container {
  align-self: center;
}

.padlock-icon {
  margin-left: 0.2rem;
}
</style>
