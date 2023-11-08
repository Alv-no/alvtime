<template>
  <div class="grid">
    <TimeEntrieText :time-entrie="timeEntrie" />
    <HourInput :value="timeEntrie.value" />
  </div>
</template>

<script lang="ts">
import {defineComponent} from "vue";
import TimeEntrieText from "./TimeEntrieText.vue";
import HourInput from "./HourInput.vue";
import { FrontendTimentrie } from "@/store/timeEntries";

export default defineComponent({
  components: {
    TimeEntrieText,
    HourInput,
  },
  props: {
    timeEntrie: {
      type: Object as () => FrontendTimentrie,
      default: (): FrontendTimentrie => {
        return {} as FrontendTimentrie;
      },
    },
  },
});
</script>

<style scoped>
.grid {
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
  color: #000;
  padding: 0 1rem;
}
</style>
