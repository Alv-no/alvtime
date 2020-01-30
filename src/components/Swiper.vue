<template>
  <div>
    <mq-layout mq="sm">
      <h2>{{ day }}</h2>
      <swiper :options="swiperOption" ref="mySwiper">
        <swiperSlide v-for="(date, index) in dates" :key="index">
          <TimeEntrieDayList :date="date" />
        </swiperSlide>
      </swiper>
    </mq-layout>
    <mq-layout mq="md+">
      <swiper :options="swiperOption" ref="mySwiper">
        <swiperSlide v-for="(week, index) in weeks" :key="index">
          <TimeEntrieWeekList :week="week" />
        </swiperSlide>
        <div class="swiper-button-prev" slot="button-prev"></div>
        <div class="swiper-button-next" slot="button-next"></div>
      </swiper>
    </mq-layout>
    <md-snackbar
      :md-position="position"
      :md-duration="duration"
      :md-active.sync="updateExists"
      md-persistent
    >
      <span>New version available! Click to update</span>
      <md-button class="md-primary" @click="refreshApp">Retry</md-button>
    </md-snackbar>
  </div>
</template>

<script>
import "swiper/dist/css/swiper.css";
import moment from "moment";

import { swiper, swiperSlide } from "vue-awesome-swiper";
import TimeEntrieDayList from "./TimeEntrieDayList";
import TimeEntrieWeekList from "./TimeEntrieWeekList";

export default {
  components: {
    swiper,
    swiperSlide,
    TimeEntrieDayList,
    TimeEntrieWeekList,
  },

  data() {
    const vueComponent = this;
    return {
      duration: 60000,
      position: "center",
      refreshing: false,
      registration: null,
      updateExists: false,
      swiperOption: {
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

    day() {
      if (this.dates.length) {
        const d = this.dates[this.$store.state.activeSlideIndex].format(
          "dddd DD. MMMM"
        );
        return d.charAt(0).toUpperCase() + d.slice(1);
      }
      return "";
    },
  },

  created() {
    document.addEventListener("swUpdated", this.showRefreshUI, { once: true });
    navigator.serviceWorker.addEventListener("controllerchange", () => {
      if (this.refreshing) return;
      this.refreshing = true;
      window.location.reload();
    });
  },

  mounted() {
    this.dates = createDays();
    this.weeks = createWeeks();
    this.swiper.slideTo(3, 1000, false);
  },

  methods: {
    showRefreshUI(e) {
      this.registration = e.detail;
      this.updateExists = true;
    },

    refreshApp() {
      this.updateExists = false;
      if (!this.registration || !this.registration.waiting) {
        return;
      }
      this.registration.waiting.postMessage("skipWaiting");
    },
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
