<template>
  <div class="outer-div">
    <div class="inline-block">
      <div class="holiday holiday-text">{{ holiday }}</div>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
export default Vue.extend({
  props: {
    holiday: {
      type: String,
      default: "",
    },
  },
});
</script>

<style scoped>
.holiday {
  background-color: #f5f5f5;
}

.outer-div {
  padding: 5px 1rem;
  text-align: center;
}

.inline-block {
  display: inline-block;
  margin: 0 auto;
}

.holiday-text {
  color: white;
  text-align: center;
  padding: 0rem 1rem;
  vertical-align: middle;
  line-height: 2rem;
  font-weight: 600;
  background-color: #f39123;
  border-radius: 30px;
}
</style>
