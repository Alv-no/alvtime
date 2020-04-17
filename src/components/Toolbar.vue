<template>
  <div class="grid">
    <div class="hour_sums"><HourSums /></div>
    <div class="title"><Title /></div>
    <div class="nav_buttons"><NavButtons /></div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import NavButtons from "@/components/NavButtons.vue";
import Title from "@/components/Title.vue";
import HourSums from "@/components/HourSums.vue";

export default Vue.extend({
  components: {
    NavButtons,
    Title,
    HourSums,
  },
});
</script>

<style scoped>
.grid {
  display: grid;
  width: 100%;
  grid-template-columns: 200px minmax(6rem, calc(27rem)) 200px;
  align-items: center;
  justify-content: center;
  text-align: center;
}

.hour_sums {
  justify-self: left;
  margin-left: 0.5rem;
}

.title {
  justify-self: center;
}

.nav_buttons {
  justify-self: right;
  margin-right: 0.5rem;
}

@media only screen and (max-width: 650px) {
  .grid {
    grid-template-columns: 50px minmax(6rem, calc(27rem)) 200px;
  }
}

@media only screen and (max-width: 449px) {
  .grid {
    grid-template-columns: 50px minmax(6rem, calc(27rem)) 50px;
  }
}
</style>
