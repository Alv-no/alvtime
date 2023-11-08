<template>
  <div>
    <md-dialog :md-active.sync="showDialog">
      <md-dialog-title>Gapestokk</md-dialog-title>
      <div class="body">
        <h4>SHAME 🔔 SHAME 🔔 SHAME 🔔</h4>
        <img class="light logo" src="/img/niclas.png" alt="Hvit Alv-logo" />
        <p>
          Niclas har under denne hjemmekontor tiden hatt vanskeligheter med å
          møte tidsnokk til den daglige Alvtime standupen kl 10:00.
        </p>
        <p>
          Vi i Alvtime gruppen har valgt å "hjelpe" Niclas på følgende måte.
          Først og fremst ved å påpeke ironien i at prosjektlederen som
          arrangerer møtene ikke møter opp til tiden og minne han på hans egne
          sitat.
        </p>
        <blockquote cite="https://www.huxley.net/bnw/four.html">
          <i
            >"Det verste jeg vet er folk som kommer 5 minutter for sent til
            møter."</i
          >
          <footer>- Niclas Ahlbom</footer>
        </blockquote>

        <p>
          Alvtime gruppen vil også holde telling på gjennomsnitlig antall
          minutter Niclas kommer for sent til møtene. Denne gapestokken er
          programert til å vises så lenge gjennomsnittet er over 5 minutter.
        </p>

        <p>
          Vi håper også at du som den
          <strong>ærlige, rå{{ " " }}</strong
          >og
          <strong>engasjerte</strong>
          Alven du er, vil hjelpe Niclas igjennom dette ved å minne han på hans
          egne sitat ved første anledning.
        </p>

        <p>
          (Trykk på Alv logoen og last siden på nytt dersom du vil se
          gapestokken igjen.)
        </p>

        <md-dialog-actions>
          <button class="md-primary" @click="onDialogActionClick">{{
            actionText
          }}</button>
        </md-dialog-actions>
      </div>
    </md-dialog>
  </div>
</template>

<script lang="ts">
import Vue from "vue";

export default Vue.extend({
  data: () => ({
    showDialog: true,
  }),

  computed: {
    actionText() {
      const { userName, name } = this.$store.state.account;
      console.log("userName: ", userName);
      return userName === "niclas@alv.no"
        ? `Mitt navn er ${name} og jeg skammer meg!`
        : `Jeg ${name}, lover å minne Niclas Ahlbom på dette, ved først anledning!`;
    },
  },

  created() {
    this.showDialog = !this.$store.state.dontShowGapestokk;
  },

  methods: {
    onDialogActionClick() {
      this.showDialog = false;
      this.$store.commit("SET_DONT_SHOW_GAPESTOKK", true);
    },
  },
});
</script>

<style scoped>
.body {
  padding: 0 1rem;
  padding-bottom: 1rem;
  overflow: auto;
  max-width: calc(100vw);
}

img {
  max-width: calc(100vw - 2rem);
}

@media only screen and (min-width: 600px) {
  img {
    max-width: 300px;
    float: left;
    margin-right: 1rem;
  }
}

.body >>> .md-button,
.md-button-clean {
  white-space: unset;
}

.body >>> .md-button {
  text-transform: unset;
}
</style>
