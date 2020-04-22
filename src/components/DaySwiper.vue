<template>
  <div>
    <div id="day-swiper-container" ref="mySwiper" class="swiper-container">
      <div class="swiper-wrapper">
        <div
          v-for="(date, index) in virtualData.slides"
          :key="index"
          class="swiper-slide"
          :style="{ left: `${virtualData.offset}px` }"
        >
          <TimeEntrieDayList :date="date" />
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import { Moment } from "moment";
import TimeEntrieDayList from "./TimeEntrieDayList.vue";
import { GLOBAL_SWIPER_OPTIONS } from "@/store/swiper";

import Swiper from "swiper";

export default Vue.extend({
  components: {
    TimeEntrieDayList,
  },

  data() {
    return {
      virtualData: [] as Moment[],
    };
  },

  beforeCreate() {
    this.$store.commit("CREATE_DATES");
    this.$store.dispatch("FETCH_DATE_ENTRIES");
  },

  mounted() {
    const swiperOptions = {
      ...GLOBAL_SWIPER_OPTIONS,
      initialSlide: this.$store.getters.initialDaySlide,
      on: {
        transitionEnd: this.onTransitionEnd,
        touchStart: this.onTransitionStart,
      },
      virtual: {
        slides: this.$store.state.dates,
        renderExternal: this.onRenderExternal,
      },
    };

    const swiper = new Swiper("#day-swiper-container", swiperOptions);
    this.$store.commit("SET_SWIPER", swiper);
  },

  methods: {
    onTransitionEnd() {
      this.$store.commit("UPDATE_ACTVIE_DATE_IN_DATES");
    },

    onTransitionStart() {
      this.$store.commit("UPDATE_EDITING", false);
    },

    onRenderExternal(data: Moment[]) {
      this.virtualData = data;
    },
  },
});
</script>
