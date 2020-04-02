<template>
  <div>
    <div id="week-swiper-container" ref="mySwiper" class="swiper-container">
      <div class="swiper-wrapper">
        <div
          class="swiper-slide"
          v-for="(week, index) in virtualData.slides"
          :key="index"
          :style="{ left: `${virtualData.offset}px` }"
        >
          <TimeEntrieWeekList :week="week" />
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Moment } from "moment";
import config from "@/config";
import TimeEntrieWeekList from "./TimeEntrieWeekList.vue";
import { GLOBAL_SWIPER_OPTIONS } from "@/store/swiper";
import { createWeek } from "@/mixins/date";

import Swiper from "swiper";

export default Vue.extend({
  components: {
    TimeEntrieWeekList,
  },

  data() {
    return {
      virtualData: [[]] as Moment[][],
    };
  },

  mounted() {
    this.$store.dispatch("CREATE_WEEKS");
    const self = this;
    const swiperOptions = {
      ...GLOBAL_SWIPER_OPTIONS,
      initialSlide: 52,
      on: {
        transitionEnd: this.onTransitionEnd,
      },
      virtual: {
        slides: this.$store.state.weeks,
        renderExternal: this.onRenderExternal,
      },
    };

    const swiper = new Swiper("#week-swiper-container", swiperOptions);
    this.$store.commit("SET_SWIPER", swiper);

    this.$store.dispatch("FETCH_WEEK_ENTRIES");
  },

  methods: {
    onTransitionEnd() {
      this.$store.commit("UPDATE_ACTVIE_DATE_IN_WEEKS");
    },

    onRenderExternal(data: Moment[][]) {
      this.virtualData = data;
    },
  },

  computed: {
    dateRange():
      | { fromDateInclusive: string; toDateInclusive: string }
      | undefined {
      return this.$store.getters.dateRange;
    },
  },
});
</script>
