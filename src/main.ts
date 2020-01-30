import Vue from "vue";
import App from "./App.vue";
import "./registerServiceWorker";
import router from "./router";
import store from "./store";
import "vue-material/dist/vue-material.min.css";
import "vue-material/dist/theme/default.css";
import { MdButton, MdSnackbar } from "vue-material/dist/components";
import VueMq from "vue-mq";
import moment from "moment";
moment.locale("nb");

Vue.use(VueMq);
Vue.use(MdButton);
Vue.use(MdSnackbar);

Vue.config.productionTip = false;

new Vue({
  router,
  store,
  render: h => h(App),
}).$mount("#app");
