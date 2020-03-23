<template>
  <div>
    <FavoriteSelector v-if="selectFavorites && isTasks" />
    <div v-if="!selectFavorites && isTasks">
      <mq-layout mq="sm">
        <MobileSwiper />
      </mq-layout>
      <mq-layout mq="md+">
        <WeekSwiper />
      </mq-layout>
    </div>
    <Spinner v-if="!isTasks" />
  </div>
</template>

<script>
import moment from "moment";

import MobileSwiper from "./MobileSwiper";
import WeekSwiper from "./WeekSwiper";
import FavoriteSelector from "./FavoriteSelector";
import Spinner from "./Spinner";
import isInIframe from "@/mixins/isInIframe";

export default {
  components: {
    MobileSwiper,
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
    selectFavorites() {
      return this.$store.state.selectFavorites;
    },

    isTasks() {
      return !!this.$store.state.tasks.length;
    },
  },
};
</script>
