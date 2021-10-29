<template>
  <div id="root">
  </div>
</template>

<script lang="ts">

import Vue from "vue";

export default Vue.extend({
  components: {
  },

  computed: {
  },

  methods: {
      insertScripts() {
        const microScripts = [
        'http://localhost:3000/static/js/bundle.js', 
        'http://localhost:3000/static/js/0.chunk.js', 
        'http://localhost:3000/static/js/main.chunk.js']
        
        microScripts.forEach(uri => {
            const el = document.createElement('script')
            el.setAttribute('src', uri)
            document.querySelector('body').appendChild(el)
            })
      }
  },
  mounted() {
    this.insertScripts()
      }
});
</script>

<style scoped>
</style>
