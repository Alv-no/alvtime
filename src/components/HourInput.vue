<template>
  <form novalidate>
    <input type="number" v-on:input="onInput" :value="timeEntrie.value" />
  </form>
</template>

<script>
export default {
  props: ["timeEntrie"],
};
</script>

<style scoped>
input {
  appearance: none;
  -moz-appearance: textfield;
  width: 1.5rem;
  height: 1rem;
  padding: 5px;
}
</style>
