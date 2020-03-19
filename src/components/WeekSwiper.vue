<template>
  <div>
    <WeekHeader :week="week" />
    <swiper :options="swiperOption" ref="mySwiper">
      <swiperSlide v-for="(week, index) in weeks" :key="index">
        <TimeEntrieWeekList :week="week" />
      </swiperSlide>
      <div class="swiper-button-prev" slot="button-prev"></div>
      <div class="swiper-button-next" slot="button-next"></div>
    </swiper>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import moment from "moment";

import { swiper, swiperSlide } from "vue-awesome-swiper";
import TimeEntrieWeekList from "./TimeEntrieWeekList.vue";
import WeekHeader from "./WeekHeader.vue";

export default Vue.extend({
  components: {
    swiper,
    swiperSlide,
    TimeEntrieWeekList,
    WeekHeader,
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
        keyboard: {
          enabled: true,
          onlyInViewport: false,
        },
        navigation: {
          nextEl: ".swiper-button-next",
          prevEl: ".swiper-button-prev",
        },
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
      weeks: createWeeks(),
    };
  },

  computed: {
    swiper() {
      // @ts-ignore
      return this.$refs.mySwiper.swiper;
    },

    week() {
      // @ts-ignore
      if (this.weeks.length) {
        // @ts-ignore
        return this.weeks[this.$store.state.activeSlideIndex];
      }
      return "";
    },
  },
});

function createWeeks() {
  return [-3, -2, -1, 0, 1, 2, 3]
    .map(n => moment().add(n, "week"))
    .map(createWeek);
}

function createWeek(day: moment.Moment) {
  const monday = day.startOf("week");
  return [0, 1, 2, 3, 4, 5, 6].map(n => monday.clone().add(n, "day"));
}
</script>
