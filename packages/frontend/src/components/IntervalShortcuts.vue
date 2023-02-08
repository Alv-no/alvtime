<template>
    <div>
      <button @click="onIntervalClicked()">Januar</button>
    </div>
  </template>
  
  <script lang="ts">
  import Vue from "vue";
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
            this.fromDate = '2023-01-01';
            this.toDate = '2023-01-31';
            this.$emit("januarSelected", { fromDate: this.fromDate, toDate: this.toDate});
        },
    },
  });
  </script>
  
  <style scoped>

  </style>
  