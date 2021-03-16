<template>
  <CenterColumnWrapper>
    <div class="padding">
      <div class="vacation-used">
        <div>
          <b>Ferietid tatt ut så langt i {{ currentYear }}:</b>
          {{ daysUsed }} dager og {{ hoursUsed }} timer
        </div>
      </div>
    </div>
  </CenterColumnWrapper>
</template>

<script lang="ts">
import Vue from "vue";
import moment, { Moment } from "moment";
import CenterColumnWrapper from "@/components/CenterColumnWrapper.vue";

export default Vue.extend({
  components: {
    CenterColumnWrapper,
  },
  computed: {
    hoursUsed(): number {
      return this.$store.getters.getUsedVacationHours;
    },
    daysUsed(): number {
      return this.$store.getters.getUsedVacationDays;
    },
    currentYear(): number {
      return moment().year();
    },
  },

  async created() {
    await this.$store.dispatch("FETCH_USED_VACATION", {
      year: this.currentYear,
    });
  },
});
</script>

<style scoped>
.padding {
  padding: 1rem;
}
.vacation-used b {
  font-size: 0.7rem;
  font-weight: 600;
}
.vacation-used {
  font-size: 1rem;
  font-weight: 600;
  margin-bottom: 10px;
}
</style>
