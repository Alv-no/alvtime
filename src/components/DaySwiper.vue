<template>
  <div>
    <DayHeader />
    <div ref="mySwiper" class="swiper-container">
      <div class="swiper-wrapper">
        <div
          class="swiper-slide"
          v-for="(date, index) in virtualData.slides"
          :key="index"
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
import config from "@/config";
import TimeEntrieDayList from "./TimeEntrieDayList.vue";
import DayHeader from "./DayHeader.vue";
import isInIframe from "@/mixins/isInIframe";
import moment from "moment";

import Swiper from "swiper";

export default Vue.extend({
  components: {
    TimeEntrieDayList,
    DayHeader,
  },

  data() {
    return {
      dates: [] as moment.Moment[],
      virtualData: [] as moment.Moment[],
      swiper: {} as Swiper,
    };
  },

  mounted() {
    this.dates = this.createManySlides();
    const self = this;
    this.swiper = new Swiper(".swiper-container", {
      initialSlide: 364,
      shortSwipes: false,
      simulateTouch: false,
      noSwipingSelector: "input, button",
      longSwipesRatio: 0.15,
      longSwipesMs: 100,
      keyboard: {
        enabled: true,
        onlyInViewport: false,
      },
      on: {
        transitionEnd: self.onTransitionEnd,
        touchStart: self.onTransitionStart,
      },
      virtual: {
        slides: self.dates,
        renderExternal(data: moment.Moment[]) {
          self.virtualData = data;
        },
      },
    });

    if (!isInIframe()) {
      this.$store.dispatch("FETCH_TIME_ENTRIES", this.dateRange);
    }
  },

  computed: {
    dateRange(): { fromDateInclusive: string; toDateInclusive: string } {
      return {
        fromDateInclusive: this.dates[0].format(config.DATE_FORMAT),
        toDateInclusive: this.dates[this.dates.length - 1].format(
          config.DATE_FORMAT
        ),
      };
    },
  },

  methods: {
    onTransitionEnd() {
      const activeDate = this.dates[this.swiper.activeIndex];
      const date = activeDate ? activeDate : moment();
      this.$store.commit("UPDATE_ACTVIE_DATE", date);
    },

    onTransitionStart() {
      this.$store.commit("UPDATE_EDITING", false);
    },

    createManySlides(): moment.Moment[] {
      const date = this.$store.state.activeDate;
      const future = Array.apply(null, Array(730)).map((n, i) => i);
      const past = Array.apply(null, Array(365))
        .map((n, i) => (i + 1) * -1)
        .reverse();
      return [...past, ...future].map(n => date.clone().add(n, "day"));
    },
  },
});
</script>
