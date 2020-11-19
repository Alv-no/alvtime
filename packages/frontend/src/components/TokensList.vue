<template>
  <div class="container">
    <div v-if="!!tokens.length">
      <div class="flush-right">
        <YellowButton
          icon-id="delete_forever"
          tooltip="Slett alle"
          text="Fjern alle"
          @click="onDeletAllClick"
        />
      </div>
      <div class="row header">
        <div class="friendly-name">Navn</div>
        <div class="expiry-date-header">Utløpsdato</div>
        <div />
      </div>
      <div class="line" />
    </div>
    <div v-for="tkn in prettyTokens" :key="tkn.id" class="row">
      <div class="friendly-name">{{ tkn.friendlyName }}</div>
      <div class="expiry-date">{{ tkn.expiryDate }}</div>
      <YellowButton
        icon-id="delete_forever"
        tooltip="Slett"
        @click="() => onDeleteClick(tkn)"
      />
    </div>
    <md-dialog-confirm
      :md-active.sync="active"
      md-title="Slette tokens?"
      md-content="Er du sikker på at du vil slette tokens? De vil da ikke lenger fungere på steder du har implementert dem."
      md-confirm-text="Slett"
      md-cancel-text="Avbryt"
      @md-confirm="onConfirm"
    />
  </div>
</template>

<script lang="ts">
import Vue from "vue";
import moment from "moment";
import YellowButton from "./YellowButton.vue";
import { adAuthenticatedFetch } from "@/services/auth";
import config from "@/config";

interface Token {
  id: number;
  friendlyName: string;
  expiryDate: string;
}

export default Vue.extend({
  components: {
    YellowButton,
  },

  data() {
    return {
      tokens: [] as Token[],
      active: false,
      token: null as null | Token,
    };
  },

  computed: {
    prettyTokens(): Token[] {
      return this.tokens.map((token: Token) => {
        const { id, friendlyName, expiryDate } = token;
        const humanReadableDate = moment(expiryDate).format(
          "dddd D. MMMM YYYY"
        );
        return { id, friendlyName, expiryDate: humanReadableDate };
      });
    },
  },

  created() {
    this.fetchActiveAccessTokens();
  },

  methods: {
    onDeleteClick(token: Token) {
      this.active = true;
      this.token = token;
    },

    onDeletAllClick() {
      this.active = true;
    },

    onConfirm() {
      if (this.token != null) {
        this.deleteAccessTokens([this.token]);
        this.token = null;
      } else {
        this.deleteAccessTokens(this.tokens);
      }
    },

    async fetchActiveAccessTokens() {
      try {
        const url = new URL(
          config.API_HOST + "/api/user/ActiveAccessTokens"
        ).toString();
        const res = await adAuthenticatedFetch(url);
        if (res.status !== 200) {
          throw res.statusText;
        }
        const tokens = await res.json();
        this.tokens = tokens;
      } catch (e) {
        console.error(e);
        this.$store.commit("ADD_TO_ERROR_LIST", e);
      }
    },

    async deleteAccessTokens(tokens: { id: number }[]) {
      try {
        const method = "delete";
        const headers = { "Content-Type": "application/json" };
        const tokensToDelete = tokens.map(token => ({ tokenId: token.id }));
        const body = JSON.stringify(tokensToDelete);
        const options = { method, headers, body };

        const response = await adAuthenticatedFetch(
          config.API_HOST + "/api/user/AccessToken",
          options
        );

        if (response.status !== 200) {
          throw response.statusText;
        }

        const deletedTokens = await response.json();
        this.tokens = this.tokens.filter(
          (token: Token) =>
            !deletedTokens.some(
              (deletedToken: { id: number }) => deletedToken.id === token.id
            )
        );
      } catch (e) {
        console.error(e);
        this.$store.commit("ADD_TO_ERROR_LIST", e);
      }
    },
  },
});
</script>

<style scoped>
.row {
  display: grid;
  grid-template-columns: 1fr 1fr 60px;
  align-items: center;
  color: #000;
  padding: 0 1rem;
  grid-gap: 0.5rem;
}

.container {
  padding-top: 1rem;
}

.friendly-name {
  font-weight: 600;
  text-transform: capitalize;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.expiry-date {
  text-transform: capitalize;
}

.expiry-date-header {
  font-weight: 600;
}

.line {
  border: 0.5px solid #008dcf;
  margin: 0.3rem 1rem;
}

.flush-right {
  display: grid;
  margin-right: 1rem;
  justify-content: right;
}
</style>
