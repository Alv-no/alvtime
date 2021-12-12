<template>
  <div class="entry-container" @click="$emit('expand-entry', task.id)">
    <div class="entry-text" v-if="!isExpanded">
      <span class="customer-name truncate-text">{{ task.project.customer.name }} - {{ task.project.name }}</span>  
      <span class="activity-name truncate-text">{{ task.name }}</span>
    </div>
    <div class="entry-text-expanded" v-if="isExpanded">
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
    </div>
    <div v-show="showPadlock" class="padlock-container">
      <md-icon class="padlock-icon">lock</md-icon>
    </div>
    <div class="rate-container"  v-if="!isExpanded">
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
    isExpanded: Boolean
  },
  computed: {
    compensationRatePercentage(): string {
      return `${this.task.compensationRate * 100}%`;
    },
    showPadlock(): boolean {
      return this.task.locked;
    }
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
  margin-right: .4rem;
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
  margin-top: .4rem;
  margin-bottom: .4rem;
  word-break: break-word;
}

.expanded-label {
  font-weight: 600;
}
.expanded-text {
  font-weight: normal;
  font-size: 0.8rem;
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
  margin-left: .2rem;
}

.rate-text {
  justify-self: right;
  font-weight: lighter;
}

.padlock-container {
  align-self: center;
  
}

.padlock-icon {
  margin-left: .2rem;
}
</style>
