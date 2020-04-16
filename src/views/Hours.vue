<template>
  <div>
    <Progress :visible="progressBarVisible" />
    <div class="swipers">
      <mq-layout mq="sm">
        <DaySwiper />
      </mq-layout>
      <mq-layout mq="md+">
        <WeekSwiper />
      </mq-layout>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";

import DaySwiper from "@/components/DaySwiper.vue";
import WeekSwiper from "@/components/WeekSwiper.vue";
import Progress from "@/components/Progress.vue";

export default Vue.extend({
  name: "hours",

  components: {
    DaySwiper,
    WeekSwiper,
    Progress,
  },

  computed: {
    progressBarVisible() {
      console.log(
        "this.$store.state.timeEntries: ",
        this.$store.state.timeEntries
      );
      return this.$store.state.timeEntries === null;
    },
  },
});
</script>

<style scoped>
.swipers {
  padding-top: 1rem;
}

.hidden {
  visibility: hidden;
}
</style>
