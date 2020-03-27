<template>
  <div>
    <FavoriteSelector v-if="selectFavorites && isTasks" />
    <div v-if="!selectFavorites && isTasks">
      <mq-layout mq="sm">
        <DaySwiper />
      </mq-layout>
      <mq-layout mq="md+">
        <WeekSwiper />
      </mq-layout>
    </div>
    <Spinner v-if="!isTasks" />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import moment from "moment";

import DaySwiper from "./DaySwiper.vue";
import WeekSwiper from "./WeekSwiper.vue";
import FavoriteSelector from "./FavoriteSelector.vue";
import Spinner from "./Spinner.vue";
import isInIframe from "@/mixins/isInIframe";

export default Vue.extend({
  components: {
    DaySwiper,
    WeekSwiper,
    FavoriteSelector,
    Spinner,
  },

  created() {
    if (!isInIframe()) {
      this.$store.dispatch("FETCH_TASKS");
    }
  },

  computed: {
    selectFavorites(): boolean {
      return this.$store.state.selectFavorites;
    },

    isTasks(): boolean {
      return !!this.$store.state.tasks.length;
    },
  },
});
</script>
