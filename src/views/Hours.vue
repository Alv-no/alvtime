<template>
  <div>
    <div v-if="!$store.state.timeEntries.length">
      <mq-layout mq="sm">
        <DaySwiper />
      </mq-layout>
      <mq-layout mq="md+">
        <WeekSwiper />
      </mq-layout>
    </div>
    <Spinner v-if="$store.state.timeEntries.length" />
  </div>
</template>

<script lang="ts">
import Vue from "vue";

import DaySwiper from "@/components/DaySwiper.vue";
import WeekSwiper from "@/components/WeekSwiper.vue";
import Spinner from "@/components/Spinner.vue";

export default Vue.extend({
  name: "hours",
  components: {
    DaySwiper,
    WeekSwiper,
    Spinner,
  },
});
</script>
