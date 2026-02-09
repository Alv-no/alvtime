<template>
<div v-if="!loading">
  <h1>Personal access tokens</h1>
  <div class="description">
    Personlige access token fungerer akkurat som OAuth tilgangstoken for å
    autentisere deg mot Alvtime API. Bruk de som bearer tokens i
    applikasjoner der det ikke er mulig eller praktisk å implementere login
    på vanlig måte.
  </div>
  <div class="form-container">
    <div class="form-row">
    <input
      id="create-pat"
      v-model="tokenName"
      type="string"
      placeholder="Navn på nytt token"
      @focus="onInputFocus"
    />
    <button
      :disabled="tokenName.length <= 0"
      @click="createToken"
    >
      Lag token
    </button>
    </div>
    <div v-if="temporaryVisibleToken" class="new-token-container">
      <b>Kopiér opprettet token og lagre det. Tokenet vil ikke kunne vises på nytt.</b>
      <p>Token: <b>{{ temporaryVisibleToken.token }}</b></p>
      <p>Utgår: {{ formatDate(temporaryVisibleToken.expiryDate) }}</p>
      <div class="copy-button-wrapper">

      <AlvtimeButton iconRight @click="copyTokenToClipboard">
        Kopiér <FeatherIcon name="clipboard" />
      </AlvtimeButton>
      <span v-if="showCopiedTooltip" class="copied-tooltip">Kopiert!</span>
      </div>
    </div>
  </div>
  <div class="table-container">
    <h3>Eksisterende tokens</h3>
    <table class="token-table">
      <thead>
      <tr>
        <th>Navn</th>
        <th>Utgår</th>
        <th></th>
      </tr>
      </thead>
      <tbody>
      <tr v-for="token in accessTokens" :key="token.friendlyName">
        <td>{{ token.friendlyName }}</td>
        <td>{{ formatDate(token.expiryDate) }}</td>
        <td class="action-cell">
          <button class="delete-btn" @click="deleteToken(token.id)" title="Slett token">
            <FeatherIcon name="trash-2" />
          </button>
        </td>
      </tr>
      <tr v-if="accessTokens.length === 0">
        <td colspan="2">Ingen tokens funnet</td>
      </tr>
      </tbody>
    </table>
  </div>
</div>
</template>

<script setup lang="ts">
import { type CreatedTokenResponse, useAccessTokenStore } from "@/stores/accessTokenStore.ts";
import { storeToRefs } from "pinia";
import { onMounted, ref } from "vue";
import AlvtimeButton from "@/components/utils/AlvtimeButton.vue";
import FeatherIcon from "@/components/utils/FeatherIcon.vue";

const accessTokenStore = useAccessTokenStore();
const { accessTokens } = storeToRefs(accessTokenStore);
const loading = ref<boolean>(true);
const tokenName = ref<string>("");
let temporaryVisibleToken = ref<CreatedTokenResponse | null>(null);
const showCopiedTooltip = ref(false);

const createToken = async () => {
  const createdToken = await accessTokenStore.createAccessToken(tokenName.value);
  if (createdToken){
    temporaryVisibleToken.value = createdToken;
    tokenName.value = "";
  }
}

const deleteToken = async (tokenId: number) => {
  await accessTokenStore.deleteAccessToken(tokenId);
}

const onInputFocus = () => {
  const inputElement = document.getElementById("create-pat") as HTMLInputElement;
  inputElement.select();
};

const copyTokenToClipboard = () => {
  if (temporaryVisibleToken.value) {
    navigator.clipboard.writeText(temporaryVisibleToken.value.token);
    showCopiedTooltip.value = true;
    setTimeout(() => {
      showCopiedTooltip.value = false;
    }, 2500);
  }
}

function formatDate(date: Date): string {
  return new Date(date).toLocaleDateString("nb-NO", {
    day: "numeric",
    month: "short",
    year: "numeric"
  });
}

onMounted( async () => {
  await accessTokenStore.getAccessTokens();
  loading.value = false;
});
</script>

<style scoped lang="scss">
.form-container {
  margin-top: 16px;
  margin-bottom: 16px;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
}

.form-row {
  display: flex;
  gap: 16px;
  width: 100%;

  input {
    padding: 0.5rem;
    border: 1px solid #ccc;
    border-radius: 25px;
    flex: auto;
    text-align: center;
  }

  button {
    background-color: $secondary-color;
    color: $primary-color;
    border-radius: 25px;
    border: none;
    padding: 13px 24px 13px 24px;
    cursor: pointer;
    font-size: 14px;
    font-weight: 600;

    &:hover {
      background-color: $secondary-color-light;
      color: $primary-color;
    }

    &:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }
  }
}

.new-token-container {
  padding: 1rem;
  background-color: $secondary-color-light;
  border-radius: 0.5rem;
}

.table-container {
  padding: 0.2rem 0.5rem 1.0rem 0.5rem;
  display: flex;
  flex-direction: column;
  border: 3px solid $secondary-color;
  border-radius: 0.5rem;
  margin-bottom: 0.5rem;
}

.token-table {
  width: 100%;
  border-collapse: collapse;

  th, td {
    padding: 0.75rem;
    border-bottom: 1px solid #e0e0e0;
  }

  th {
    font-weight: bold;
    background-color: $secondary-color-light;
  }

  td {
    text-align: center;
  }

  .action-cell {
    width: 1%;
    white-space: nowrap;
    padding: 0.5rem;
  }
}


.delete-btn {
  background: none;
  border: none;
  cursor: pointer;
  padding: 0.4rem;
  color: #888;
  border-radius: 4px;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: color 0.2s, background-color 0.2s;

  &:hover {
    color: #c53030;
    background-color: rgba(197, 48, 48, 0.1);
  }
}

.copy-button-wrapper {
  position: relative;
  display: inline-block;
}

.copied-tooltip {
  position: absolute;
  bottom: 100%;
  left: 50%;
  transform: translateX(-50%);
  background-color: #333;
  color: #fff;
  padding: 6px 12px;
  border-radius: 4px;
  font-size: 13px;
  white-space: nowrap;
  margin-bottom: 8px;

  &::after {
    content: '';
    position: absolute;
    top: 100%;
    left: 50%;
    transform: translateX(-50%);
    border: 6px solid transparent;
    border-top-color: #333;
  }
}

.description {
  margin: 1rem auto;
  max-width: 48rem;
  padding: 1rem 1.5rem;
  background-color: rgba($secondary-color-light, 0.3);
  border-left: 4px solid $secondary-color;
  border-radius: 0 0.5rem 0.5rem 0;
  color: #555;
  font-size: 0.95rem;
  line-height: 1.6;
}
</style>