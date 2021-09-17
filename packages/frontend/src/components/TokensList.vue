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
      <div
        class="friendly-name"
        :class="{ outdated: isExpired(tkn.expiryDate) }"
      >
        {{ tkn.friendlyName }}
      </div>
      <div class="expiry-date" :class="{ outdated: isExpired(tkn.expiryDate) }">
        {{ tkn.expiryDate }}
      </div>
      <YellowButton
        v-if="!isExpired(tkn.expiryDate)"
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
import config from "@/config";
import httpClient from "../services/httpClient";

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
    isExpired(date: string) {
      const today = moment();
      return moment(date, "dddd D. MMMM YYYY").isBefore(today);
    },

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
      httpClient
        .get<Token[]>(`${config.API_HOST}/api/user/ActiveAccessTokens`)
        .then(response => {
          this.tokens = response.data;
        });
    },

    async deleteAccessTokens(tokens: { id: number }[]) {
      httpClient
        .delete<{ id: number }[]>(`${config.API_HOST}/api/user/AccessToken`, {
          data: tokens.map(token => ({ tokenId: token.id })),
        })
        .then(response => {
          this.tokens = this.tokens.filter(
            (token: Token) =>
              !response.data.some(deletedToken => deletedToken.id === token.id)
          );
        });
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
  height: 2.5rem;
}

.header {
  height: 1rem;
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

.outdated {
  text-decoration-line: line-through;
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
