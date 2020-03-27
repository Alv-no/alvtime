import Vue from "vue";
import Vuex from "vuex";
import App from "./App.vue";
import "swiper/css/swiper.css";
import "./registerServiceWorker";
import router from "./router";
import storeOptions from "./store";
import lifecycle from "@/services/lifecycle.es5.js";
import "vue-material/dist/vue-material.min.css";
import "vue-material/dist/theme/default.css";
import { setRedirectCallback } from "@/services/auth";
import {
  MdButton,
  MdSnackbar,
  MdIcon,
  MdCheckbox,
  MdTooltip,
  MdProgress,
} from "vue-material/dist/components";
import VueMq from "vue-mq";
import VueClipboard from "vue-clipboard2";

Vue.use(Vuex);
Vue.use(VueMq);
Vue.use(MdButton);
Vue.use(MdSnackbar);
Vue.use(MdIcon);
Vue.use(MdCheckbox);
Vue.use(MdTooltip);
Vue.use(MdProgress);
Vue.use(VueClipboard);

Vue.config.productionTip = false;

const store = new Vuex.Store(storeOptions);

setRedirectCallback((errorMessage: Error) =>
  store.commit("ADD_TO_ERROR_LIST", errorMessage)
);

lifecycle.addEventListener("statechange", function(event: any) {
  store.commit("UPDATE_APP_STATE", event);
});

new Vue({
  router,
  store,
  render: h => h(App),
}).$mount("#app");
