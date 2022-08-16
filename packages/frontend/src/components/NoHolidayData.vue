<template>
  <div class="box">
    <md-icon class="icon">information</md-icon>
    <div class="text">
      <b>Ingen feriedata å vise.</b>
      <p>
        Mest sannsynlig har du ikke opptjent eller brukt noen feriedager enda.
      </p>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";

export default Vue.extend({});
</script>

<style scoped>
.box {
  box-sizing: border-box;
  display: flex;

  height: 50px;
  font-size: 12pt;
  padding: 5px;
  align-items: center;
  width: 100%;

  border: 2px solid #eabb26;
  border-radius: 10px;
}

.text p {
  margin: 0px;
}
.text {
  margin-left: 7px;
}

.icon {
  display: block;
  margin: 1rem;
}
</style>
