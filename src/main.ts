import Vue from "vue";
import App from "./App.vue";
import "swiper/dist/css/swiper.css";
import "./registerServiceWorker";
import router from "./router";
import store from "./store";
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
import moment from "moment";

moment.locale("nb");

Vue.use(VueMq);
Vue.use(MdButton);
Vue.use(MdSnackbar);
Vue.use(MdIcon);
Vue.use(MdCheckbox);
Vue.use(MdTooltip);
Vue.use(MdProgress);

Vue.config.productionTip = false;

setRedirectCallback((errorMessage: Error) =>
  store.commit("ADD_TO_ERROR_LIST", errorMessage)
);

new Vue({
  router,
  store,
  render: h => h(App),
}).$mount("#app");
