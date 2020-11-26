<template>
  <div>
    <md-datepicker v-model="selectedDate">
      <label>Select date</label>
    </md-datepicker>
  </div>
</template>

<script>
import Vue from "vue";
export default Vue.extend({
  name: "LabeledDatepicker",
  data: () => ({
    selectedDate: null,
  }),
});
</script>
