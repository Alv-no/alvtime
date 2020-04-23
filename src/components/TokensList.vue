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
    <div v-for="token in prettyTokens" :key="token.id" class="row">
      <div class="friendly-name">{{ token.friendlyName }}</div>
      <div class="expiry-date">{{ token.expiryDate }}</div>
      <YellowButton
        icon-id="delete_forever"
        tooltip="Slett"
        @click="() => onDeleteClick(token)"
      />
    </div>
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
    };
  },

  computed: {
    prettyTokens(): Token[] {
      return this.tokens.map((token: Token) => {
        const { id, friendlyName, expiryDate } = token;
        const humanReadableDate = moment(expiryDate).format("dddd D. MMMM");
        return { id, friendlyName, expiryDate: humanReadableDate };
      });
    },
  },

  created() {
    this.fetchActiveAccessTokens();
  },

  methods: {
    onDeleteClick(token: Token) {
      this.deleteAccessTokens([token]);
    },

    onDeletAllClick() {
      this.deleteAccessTokens(this.tokens);
    },

    async fetchActiveAccessTokens() {
      try {
        const url = new URL(
          config.HOST + "/api/user/ActiveAccessTokens"
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
          config.HOST + "/api/user/AccessToken",
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
  display: grid;
  padding-top: 1rem;
}

@media only screen and (min-width: 1000px) {
  .container {
    justify-content: center;
  }

  .row {
    width: 50rem;
  }
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
