<template>
  <div>
    <label>Select date</label>
    <input type="text" v-model="selectedDate">
    </input>
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
