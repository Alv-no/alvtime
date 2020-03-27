<template>
  <div>
    <WeekHeader @backClick="onPrev" @forwardClick="onNext" />
    <div ref="mySwiper" class="swiper-container">
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
import moment from "moment";
import config from "@/config";
import TimeEntrieWeekList from "./TimeEntrieWeekList.vue";
import WeekHeader from "./WeekHeader.vue";
import isInIframe from "@/mixins/isInIframe";
import { createWeek } from "@/mixins/date";

import Swiper from "swiper";

export default Vue.extend({
  components: {
    TimeEntrieWeekList,
    WeekHeader,
  },

  data() {
    return {
      weeks: [[]] as moment.Moment[][],
      virtualData: [[]] as moment.Moment[][],
      swiper: {} as Swiper,
    };
  },

  mounted() {
    this.weeks = this.createManyWeeks();
    const self = this;
    this.swiper = new Swiper(".swiper-container", {
      initialSlide: 142,
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
      },
      virtual: {
        slides: self.weeks,
        renderExternal(data: any) {
          self.virtualData = data;
        },
      },
    });

    if (!isInIframe() && this.dateRange) {
      this.$store.dispatch("FETCH_TIME_ENTRIES", this.dateRange);
    }
  },

  methods: {
    onNext() {
      this.swiper.slideNext();
    },

    onPrev() {
      this.swiper.slidePrev();
    },

    onTransitionEnd() {
      const dayOfWeek = this.$store.state.activeDate.weekday();
      const week = this.weeks[this.swiper.activeIndex];
      const date = week ? week[dayOfWeek] : moment();
      this.$store.commit("UPDATE_ACTVIE_DATE", date);
    },

    createManyWeeks(): moment.Moment[][] {
      const date = this.$store.state.activeDate;
      const future = Array.apply(null, Array(142)).map((n, i) => i);
      const past = Array.apply(null, Array(142))
        .map((n, i) => (i + 1) * -1)
        .reverse();
      return [...past, ...future]
        .map(n => date.clone().add(n, "week"))
        .map(createWeek);
    },
  },

  computed: {
    dateRange():
      | { fromDateInclusive: string; toDateInclusive: string }
      | undefined {
      if (!this.weeks[0][0]) {
        return;
      }
      return {
        fromDateInclusive: this.weeks[0][0].format(config.DATE_FORMAT),
        toDateInclusive: this.weeks[this.weeks.length - 1][6].format(
          config.DATE_FORMAT
        ),
      };
    },
  },
});
</script>
