<template>
  <div>
    <FavoriteSelector v-if="selectFavorites" />
    <div v-if="!selectFavorites && isTasks">
      <mq-layout mq="sm">
        <MobileHeader :day="day" />
        <swiper :options="swiperOption" ref="mySwiper">
          <swiperSlide v-for="(date, index) in dates" :key="index">
            <TimeEntrieDayList :date="date" />
          </swiperSlide>
        </swiper>
      </mq-layout>
      <mq-layout mq="md+">
        <WeekHeader :week="week" />
        <swiper :options="swiperOption" ref="mySwiper">
          <swiperSlide v-for="(week, index) in weeks" :key="index">
            <TimeEntrieWeekList :week="week" />
          </swiperSlide>
          <div class="swiper-button-prev" slot="button-prev"></div>
          <div class="swiper-button-next" slot="button-next"></div>
        </swiper>
      </mq-layout>
    </div>
  </div>
</template>

<script>
import "swiper/dist/css/swiper.css";
import moment from "moment";

import { swiper, swiperSlide } from "vue-awesome-swiper";
import TimeEntrieDayList from "./TimeEntrieDayList";
import TimeEntrieWeekList from "./TimeEntrieWeekList";
import MobileHeader from "./MobileHeader";
import FavoriteSelector from "./FavoriteSelector";
import WeekHeader from "./WeekHeader";

export default {
  components: {
    swiper,
    swiperSlide,
    TimeEntrieDayList,
    TimeEntrieWeekList,
    MobileHeader,
    FavoriteSelector,
    WeekHeader,
  },

  data() {
    const vueComponent = this;
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
          slideChange(event) {
            vueComponent.$store.commit("UPDATE_ACTVIE_SLIDE", this.activeIndex);
          },
        },
      },
      dates: [],
      weeks: [],
    };
  },

  computed: {
    swiper() {
      return this.$refs.mySwiper.swiper;
    },

    week() {
      if (this.weeks.length) {
        return this.weeks[this.$store.state.activeSlideIndex];
      }
      return "";
    },

    day() {
      if (this.dates.length) {
        const d = this.dates[this.$store.state.activeSlideIndex].format(
          "dddd DD. MMMM"
        );
        return d.charAt(0).toUpperCase() + d.slice(1);
      }
      return "";
    },

    selectFavorites() {
      return this.$store.state.selectFavorites;
    },

    isTasks() {
      return !!this.$store.state.tasks.length;
    },
  },

  mounted() {
    this.dates = createDays();
    this.weeks = createWeeks();
  },
};

function createDays() {
  return [-3, -2, -1, 0, 1, 2, 3].map(n => moment().add(n, "day"));
}

function createWeeks() {
  return [-3, -2, -1, 0, 1, 2, 3]
    .map(n => moment().add(n, "week"))
    .map(createWeek);
}

function createWeek(day) {
  const monday = day.startOf("week");
  return [0, 1, 2, 3, 4, 5, 6].map(n => monday.clone().add(n, "day"));
}
</script>
