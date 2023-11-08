<template>
  <div>
    <label>Select date</label>
    <input type="text" v-model="selectedDate">
    </input>
  </div>
</template>

<script>
import {defineComponent} from "vue";
export default defineComponent({
  name: "LabeledDatepicker",
  data: () => ({
    selectedDate: null,
  }),
});
</script>
