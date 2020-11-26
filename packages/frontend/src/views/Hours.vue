<template>
  <div>
    <Progress :visible="progressBarVisible" />
    <mq-layout mq="sm">
      <DaySwiper />
    </mq-layout>
    <mq-layout mq="md+">
      <WeekSwiper />
    </mq-layout>
  </div>
</template>

<script lang="ts">
import Vue from "vue";

import DaySwiper from "@/components/DaySwiper.vue";
import WeekSwiper from "@/components/WeekSwiper.vue";
import Progress from "@/components/Progress.vue";
import Gapestokk from "@/components/Gapestokk.vue";

export default Vue.extend({
  name: "Hours",

  components: {
    DaySwiper,
    WeekSwiper,
    Progress,
  },

  computed: {
    progressBarVisible() {
      return this.$store.state.timeEntries === null;
    },
  },
});
</script>
