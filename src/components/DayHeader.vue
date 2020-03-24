<template>
  <div class="grid">
    <EditFavoritesButton />
    <h2>{{ day }}</h2>
    <div class="sums">
      <div>{{ daySum }}/7,5</div>
      <div>{{ weekSum }}/37,5</div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import EditFavoritesButton from "./EditFavoritesButton.vue";
import config from "@/config";
import { FrontendTimentrie } from "@/store";
import { weekSum } from "@/mixins/date";
import moment from "moment";

export default Vue.extend({
  mixins: [weekSum],
  components: {
    EditFavoritesButton,
  },

  computed: {
    day() {
      // @ts-ignore
      const str = this.activeDate.format("dddd D. MMMM");
      return str.charAt(0).toUpperCase() + str.slice(1);
    },

    daySum() {
      const number = this.$store.state.timeEntries.reduce(
        (acc: number, curr: FrontendTimentrie) => {
          if (
            // @ts-ignore
            this.activeDate.format(config.DATE_FORMAT) === curr.date &&
            !isNaN(Number(curr.value))
          ) {
            return (acc = acc + Number(curr.value));
          } else {
            return acc;
          }
        },
        0
      );
      const str = number.toString().replace(".", ",");
      return str;
    },

    activeDate() {
      return this.$store.state.activeDate;
    },
  },
});
</script>

<style scoped>
.grid {
  display: grid;
  grid-template-columns: 50px auto 50px;
  align-items: center;
  text-align: center;
}

.sums {
  font-size: 0.5rem;
  line-height: 0.7rem;
}
</style>
