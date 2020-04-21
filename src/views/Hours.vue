<template>
  <div>
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

export default Vue.extend({
  name: "Hours",

  components: {
    DaySwiper,
    WeekSwiper,
  },
});
</script>
