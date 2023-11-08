<template>
  <CenterColumnWrapper>
    <h2 class="padding">Faktureringsgrad</h2>
    <invoice-statistics></invoice-statistics>

    <h2 class="padding">Timesoversikt</h2>
    <table v-if="getTimeEntriesSummed.length > 0" class="padding">
      <tr>
        <th>Task</th>
        <th
          v-for="month in getLastThreeMonths"
          :key="month.getMonth()"
        >
          {{ month.toLocaleString("no-nb", { month: "long" }) }}
        </th>
      </tr>
      <tr v-for="row in getTimeEntriesSummed" :key="row.id">
        <td>{{
          row.task
            ? `${row.task.project.customer.name} - ${row.task.name} ${row.task.project.name}`
            : "Ukjent navn"
        }}</td>
        <td
          v-for="element in row.summarizedHours"
          :key="element.date.getMonth()"
        >
          {{ element.value }}
        </td>
      </tr>
    </table>
    <p v-else class="padding">
      Kan ikke finne noen timer ført de siste 3 månedene.
    </p>
  </CenterColumnWrapper>
</template>

<script lang="ts">
import {defineComponent} from "vue";
import CenterColumnWrapper from "@/components/CenterColumnWrapper.vue";
import { EntriesSummarizedPerMonthPerTask } from "@/store/timeEntries";
import InvoiceStatistics from "../components/InvoiceStatistics.vue";

export default defineComponent({
  name: "Summarizedhours",
  components: {
    CenterColumnWrapper,
    InvoiceStatistics,
  },
  computed: {
    getTimeEntriesSummed(): EntriesSummarizedPerMonthPerTask {
      return this.$store.getters.getTimeEntriesSummarizedPerMonthPerTask;
    },

    getLastThreeMonths() {
      return this.$store.getters.getLastThreeMonthsForStatistics;
    },
  },

  beforeCreate() {
    this.$store.commit("CREATE_WEEKS");
    this.$store.dispatch("FETCH_WEEK_ENTRIES");
  },
});
</script>

<style scoped>
.padding {
  padding: 1rem;
}
</style>
