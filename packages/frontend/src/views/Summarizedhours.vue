<template>
  <CenterColumnWrapper>
    <md-table v-if="getTimeEntriesSummed.length > 0">
      <md-table-row>
        <md-table-head>Task</md-table-head>
        <md-table-head
          v-for="month in getLastThreeMonths"
          :key="month.getMonth()"
        >
          {{ month.toLocaleString("no-nb", { month: "long" }) }}
        </md-table-head>
      </md-table-row>
      <md-table-row v-for="row in getTimeEntriesSummed" :key="row.id">
        <md-table-cell>{{
          row.task
            ? `${row.task.project.customer.name} - ${row.task.name} ${row.task.project.name}`
            : "Ukjent navn"
        }}</md-table-cell>
        <md-table-cell
          v-for="element in row.summarizedHours"
          :key="element.date.getMonth()"
        >
          {{ element.value }}
        </md-table-cell>
      </md-table-row>
    </md-table>
    <p v-else>Kan ikke finne noen timer ført de siste 3 månedene.</p>
  </CenterColumnWrapper>
</template>

<script lang="ts">
import Vue from "vue";
import CenterColumnWrapper from "@/components/CenterColumnWrapper.vue";
import { EntriesSummarizedPerMonthPerTask } from "@/store/timeEntries";

export default Vue.extend({
  name: "Summarizedhours",
  components: {
    CenterColumnWrapper,
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
