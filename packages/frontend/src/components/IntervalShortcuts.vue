<template>
  <md-button class="md-primary md-raised" @click="onIntervalClicked()">{{ typeInterval }}</md-button>
</template>
  
<script lang="ts">
import Vue from "vue";
import moment, { Moment } from "moment";
export default Vue.extend({
  props: {
    defaultDate: {
      type: String,
      default: "",
    },
    label: {
      type: String,
      default: "",
    },
    typeInterval: {
      type: String,
      default: "",
    },
  },
  data() {
    return {
      currentDate: this.defaultDate || "",
      fromDate: this.defaultDate || "",
      toDate: this.defaultDate || ""
    };
  },
  methods: {
    onIntervalClicked() {

      var today = new Date();
      var month = today.getMonth() + 1;
      var year = today.getFullYear();

      switch (this.typeInterval) {
        case "Siste halvår":
          var lasthalfyear = new Date();
          lasthalfyear.setMonth(month - 6);
          this.toDate = moment(today).format("YYYY-MM-DD");
          this.fromDate = moment(lasthalfyear).format("YYYY-MM-DD");
          break;
        case "Siste år":
          var lastyear = new Date();
          lastyear.setMonth(month);
          lastyear.setFullYear(year - 1);
          this.toDate = moment(today).format("YYYY-MM-DD");
          this.fromDate = moment(lastyear).format("YYYY-MM-DD");
          break;
        case "Siste kvartal":
          var lastquarter = new Date();
          lastquarter.setMonth(month - 3)
          this.toDate = moment(today).format("YYYY-MM-DD");
          this.fromDate = moment(lastquarter).format("YYYY-MM-DD");
          break;
        default: break;
      }
      this.$emit("januarSelected", { fromDate: this.fromDate, toDate: this.toDate });
    },
  },
});
</script>
  
<style scoped>
.md-button {
  min-width: 125px;
  --md-theme-default-primary: #1c92d0;

}
</style>
  