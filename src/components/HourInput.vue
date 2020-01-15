<template>
  <form novalidate>
    <input type="number" novalidate />
  </form>
</template>

<script>
export default {
  props: ["value"],
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
