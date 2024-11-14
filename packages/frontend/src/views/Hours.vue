<template>
  <div>
    <Progress :visible="delayed" />
    <template v-if="!$store.state.loadingTasks">
      <mq-layout mq="sm">
        <DaySwiper />
      </mq-layout>
      <mq-layout mq="md+">
        <WeekSwiper />
      </mq-layout>
    </template>
  </div>
</template>

<script lang="ts">
import Vue from "vue";

import DaySwiper from "@/components/DaySwiper.vue";
import WeekSwiper from "@/components/WeekSwiper.vue";
import Progress from "@/components/Progress.vue";

export default Vue.extend({
  name: "Hours",

  components: {
    DaySwiper,
    WeekSwiper,
    Progress,
  },

  data() {
    return {
      timeout: 0,
      delayed: false,
    };
  },

  computed: {
    progressBarVisible() {
      return this.$store.state.timeEntriesLoading;
    },
  },

  watch: {
    progressBarVisible() {
      if (this.timeout) {
        clearTimeout(this.timeout);
      }
      if (this.progressBarVisible) {
        this.timeout = setTimeout(() => {
          this.delayed = true;
        }, 1000);
      } else {
        this.delayed = false;
      }
    },
  },

  created() {
    this.$store.dispatch("FETCH_TASKS");
  },
});
</script>
