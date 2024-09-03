<template>
  <div class="comment-container">
    <div class="comment-box">
      <label for="timeEntrieComment">Kommentar</label>
      <textarea
        id="timeEntrieComment"
        v-model="comment"
        :disabled="disabled"
        @input="onChange"
        @keydown.tab="onClickOutside"
      ></textarea>
    </div>
  </div>
</template>

<script lang="ts">
import Vue from "vue";

export default Vue.extend({
  props: {
    initialComment: {
      type: String,
      default: "",
    },
    disabled: {
      type: Boolean,
      default: false,
    },
  },
  data() {
    return {
      comment: this.initialComment,
    };
  },
  watch: {
    initialComment(newComment: string) {
      this.comment = newComment;
    },
  },
  methods: {
    onClickOutside() {
      this.$emit("close");
    },
    onChange() {
      this.$emit("change", this.comment);
    },
  },
});
</script>

<style scoped>
.comment-container {
  position: relative;
  display: inline-block;
}

.comment-box {
  position: absolute;
  top: 30px;
  left: -30px;
  transform: translateX(-50%);
  background: white;
  border: 1px solid #ccc;
  padding: 10px;
  margin-top: 5px;
  z-index: 9999;
  min-width: 200px;
  border-radius: 5px;
  box-shadow: 0 1px 5px rgba(0, 8, 61, 0.2);
  -webkit-filter: drop-shadow(
    0 1px 5px rgba(0, 8, 61, 0.2)
  ); /* Shadow color and size */
  -moz-box-shadow: 0 1px 5px rgba(0, 8, 61, 0.2);
  filter: drop-shadow(0 1px 5px rgba(0, 8, 61, 0.2));
}

.comment-box:after,
.comment-box:before {
  content: " ";
  position: absolute;
  bottom: 100%;
  left: 0;
  border: solid transparent;
  width: 0;
  height: 0;
  pointer-events: none;
}

.comment-box:after {
  border-color: rgba(255, 255, 255, 0);
  border-bottom-color: #ffffff; /* Arrow color */
  border-width: 19px;
  left: 50%;
  margin-left: -19px;
}

.comment-box:before {
  border-color: rgba(113, 158, 206, 0);
  border-width: 20px;
  left: 50%;
  margin-left: -20px;
}

@media only screen and (max-width: 1200px) {
  .comment-box {
    transform: translateX(-90%);
  }
  .comment-box:after {
    left: 90%;
  }
}

.comment-box label {
  display: block;
  margin-bottom: 5px;
}

.comment-box textarea {
  width: 100%;
  height: 50px;
  max-width: 350px;
  max-height: 200px;
}
</style>
