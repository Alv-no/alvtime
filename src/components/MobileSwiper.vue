<template>
  <div>
    <MobileHeader :day="day" />
    <swiper :options="swiperOption" ref="mySwiper">
      <swiperSlide v-for="(date, index) in dates" :key="index">
        <TimeEntrieDayList :date="date" />
      </swiperSlide>
    </swiper>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import moment from "moment";

import { swiper, swiperSlide } from "vue-awesome-swiper";
import TimeEntrieDayList from "./TimeEntrieDayList.vue";
import MobileHeader from "./MobileHeader.vue";

export default Vue.extend({
  components: {
    swiper,
    swiperSlide,
    TimeEntrieDayList,
    MobileHeader,
  },

  data() {
    const currentComponent = this;
    return {
      swiperOption: {
        initialSlide: 3,
        shortSwipes: false,
        simulateTouch: false,
        noSwipingSelector: "input, button",
        longSwipesRatio: 0.15,
        longSwipesMs: 100,
        on: {
          slideChange() {
            currentComponent.$store.commit(
              "UPDATE_ACTVIE_SLIDE",
              // @ts-ignore
              this.activeIndex
            );
          },
        },
      },
      dates: createDays(),
    };
  },

  computed: {
    swiper() {
      // @ts-ignore
      return this.$refs.mySwiper.swiper;
    },

    day() {
      // @ts-ignore
      if (this.dates.length) {
        // @ts-ignore
        const d = this.dates[this.$store.state.activeSlideIndex].format(
          "dddd DD. MMMM"
        );
        return d.charAt(0).toUpperCase() + d.slice(1);
      }
      return "";
    },
  },
});

function createDays() {
  return [-3, -2, -1, 0, 1, 2, 3].map(n => moment().add(n, "day"));
}
</script>
